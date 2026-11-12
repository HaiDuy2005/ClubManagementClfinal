using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClubManagement.Data;
using ClubManagement.Models;
using ClubManagement.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClubManagement.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly MinioStorageService _minioStorage;

        public AttendanceController(
            ApplicationDbContext context,
            MinioStorageService minioStorage)
        {
            _context = context;
            _minioStorage = minioStorage;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? searchName,
            string? searchStudentCode,
            string? searchActivity,
            DateTime? searchDate,
            string? searchStatus)
        {
            var query = _context.ActivityAttendances
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                query = query.Where(x => x.FullName.Contains(searchName));
            }

            if (!string.IsNullOrWhiteSpace(searchStudentCode))
            {
                query = query.Where(x => x.StudentCode.Contains(searchStudentCode));
            }

            if (searchDate.HasValue)
            {
                var date = searchDate.Value.Date;
                var nextDate = date.AddDays(1);
                query = query.Where(x => x.AttendanceDate >= date && x.AttendanceDate < nextDate);
            }

            if (!string.IsNullOrWhiteSpace(searchStatus) && searchStatus != "All")
            {
                query = query.Where(x => x.Status == searchStatus);
            }

            if (!string.IsNullOrWhiteSpace(searchActivity))
            {
                var matchingActivityIds = _context.Activities
                    .AsNoTracking()
                    .Where(a => a.ActivityName.Contains(searchActivity))
                    .Select(a => a.ActivityId);

                query = query.Where(x => matchingActivityIds.Contains(x.ActivityId));
            }

            var attendanceList = await query
                .OrderByDescending(x => x.AttendanceDate)
                .ThenByDescending(x => x.AttendanceId)
                .ToListAsync();

            ViewBag.ActivityNames = await _context.Activities
                .AsNoTracking()
                .ToDictionaryAsync(x => x.ActivityId, x => x.ActivityName);

            ViewBag.SearchName = searchName;
            ViewBag.SearchStudentCode = searchStudentCode;
            ViewBag.SearchActivity = searchActivity;
            ViewBag.SearchDate = searchDate?.ToString("yyyy-MM-dd");
            ViewBag.SearchStatus = searchStatus ?? "All";

            return View(attendanceList);
        }

        [HttpGet]
        public IActionResult Create()
        {
            LoadActivities();
            return View(new ActivityAttendance());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ActivityAttendance model,
            DateTime attendanceDate,
            IFormFile? evidenceFile)
        {
            model.FullName = model.FullName?.Trim() ?? "";
            model.StudentCode = model.StudentCode?.Trim() ?? "";
            model.ClassName = model.ClassName?.Trim() ?? "";

            if (attendanceDate == default)
            {
                ModelState.AddModelError("", "Vui lòng chọn ngày tham gia.");
            }

            bool activityExists = await _context.Activities
                .AnyAsync(x => x.ActivityId == model.ActivityId);

            if (!activityExists)
            {
                ModelState.AddModelError(nameof(model.ActivityId), "Vui lòng chọn hoạt động hợp lệ.");
            }

            if (string.IsNullOrWhiteSpace(model.ClassName))
            {
                ModelState.AddModelError(nameof(model.ClassName), "Vui lòng nhập lớp.");
            }

            if (evidenceFile == null || evidenceFile.Length == 0)
            {
                ModelState.AddModelError("evidenceFile", "Vui lòng chọn ảnh minh chứng.");
            }
            else
            {
                const long maxFileSize = 5 * 1024 * 1024;
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/webp" };

                string extension = Path.GetExtension(evidenceFile.FileName).ToLowerInvariant();

                if (evidenceFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("evidenceFile", "Ảnh minh chứng không được vượt quá 5 MB.");
                }

                if (!allowedExtensions.Contains(extension) ||
                    !allowedContentTypes.Contains(evidenceFile.ContentType.ToLowerInvariant()))
                {
                    ModelState.AddModelError("evidenceFile", "Chỉ chấp nhận ảnh JPG, JPEG, PNG hoặc WEBP.");
                }
            }

            if (!ModelState.IsValid)
            {
                LoadActivities();
                return View(model);
            }

            string tempFileName = $"{Guid.NewGuid()}{Path.GetExtension(evidenceFile!.FileName)}";
            string tempFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "temp");

            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }

            string tempFilePath = Path.Combine(tempFolder, tempFileName);
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await evidenceFile.CopyToAsync(stream);
            }

            model.AttendanceDate = attendanceDate;
            model.Status = "Pending";
            model.TrainingPoints = 0;
            model.EvidenceObjectKey = $"temp/{tempFileName}";
            model.EvidenceFileName = Path.GetFileName(evidenceFile.FileName);
            model.AdminNote = "";

            try
            {
                _context.ActivityAttendances.Add(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (System.IO.File.Exists(tempFilePath))
                {
                    System.IO.File.Delete(tempFilePath);
                }

                var detail = ex.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError("", $"Không thể lưu thông tin điểm danh vào cơ sở dữ liệu: {detail}");
                LoadActivities();
                return View(model);
            }

            TempData["AttendanceSuccess"] = "Gửi yêu cầu điểm danh thành công. Vui lòng chờ Admin duyệt.";
            return RedirectToAction(nameof(Create));
        }

        [HttpGet]
        public async Task<IActionResult> Evidence(int id)
        {
            var attendance = await _context.ActivityAttendances
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.AttendanceId == id);

            if (attendance == null || string.IsNullOrWhiteSpace(attendance.EvidenceObjectKey))
            {
                return NotFound("Không tìm thấy ảnh minh chứng.");
            }

            var extension = Path.GetExtension(attendance.EvidenceFileName ?? "").ToLowerInvariant();
            var contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };

            if (attendance.EvidenceObjectKey.StartsWith("temp/"))
            {
                var relativePath = attendance.EvidenceObjectKey.Replace('/', Path.DirectorySeparatorChar);
                var localPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", relativePath);
                if (System.IO.File.Exists(localPath))
                {
                    var bytes = await System.IO.File.ReadAllBytesAsync(localPath);
                    return File(bytes, contentType);
                }
            }

            try
            {
                var bytes = await _minioStorage.DownloadEvidenceAsync(attendance.EvidenceObjectKey);
                return File(bytes, contentType);
            }
            catch (Exception)
            {
                return Problem(
                    title: "Không thể đọc ảnh minh chứng từ MinIO.",
                    statusCode: 500);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(
            int id,
            int trainingPoints,
            string? adminNote)
        {
            var attendance = await _context.ActivityAttendances
                .FirstOrDefaultAsync(x => x.AttendanceId == id);

            if (attendance == null)
            {
                return NotFound();
            }

            if (attendance.Status != "Pending")
            {
                TempData["AttendanceError"] = "Yêu cầu này đã được xử lý trước đó.";
                return RedirectToAction(nameof(Index));
            }

            if (trainingPoints != 1)
            {
                TempData["AttendanceError"] = "Mỗi lần duyệt chỉ được cộng 1 điểm rèn luyện.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(attendance.FullName) ||
                string.IsNullOrWhiteSpace(attendance.StudentCode) ||
                string.IsNullOrWhiteSpace(attendance.ClassName))
            {
                TempData["AttendanceError"] = "Yêu cầu thiếu họ tên, mã sinh viên hoặc lớp.";
                return RedirectToAction(nameof(Index));
            }

            string fullName = attendance.FullName.Trim();
            string studentCode = attendance.StudentCode.Trim();
            string className = attendance.ClassName.Trim();

            var members = await _context.Members
                .Where(m =>
                    m.FullName.Trim() == fullName &&
                    m.StudentCode.Trim() == studentCode &&
                    m.ClassName.Trim() == className)
                .Take(2)
                .ToListAsync();

            if (members.Count != 1)
            {
                TempData["AttendanceError"] = members.Count == 0
                    ? "Không tìm thấy thành viên khớp họ tên, mã sinh viên và lớp trong danh sách thành viên."
                    : "Có nhiều thành viên trùng thông tin. Vui lòng kiểm tra dữ liệu.";

                return RedirectToAction(nameof(Index));
            }

            if (attendance.EvidenceObjectKey?.StartsWith("temp/") == true)
            {
                var relativePath = attendance.EvidenceObjectKey.Replace('/', Path.DirectorySeparatorChar);
                var localPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", relativePath);
                if (System.IO.File.Exists(localPath))
                {
                    try
                    {
                        using var stream = System.IO.File.OpenRead(localPath);
                        var extension = Path.GetExtension(attendance.EvidenceFileName ?? "").ToLowerInvariant();
                        var contentType = extension switch
                        {
                            ".jpg" or ".jpeg" => "image/jpeg",
                            ".png" => "image/png",
                            ".webp" => "image/webp",
                            _ => "application/octet-stream"
                        };

                        var formFile = new FormFile(stream, 0, stream.Length, "evidenceFile", attendance.EvidenceFileName ?? "evidence.jpg")
                        {
                            Headers = new HeaderDictionary(),
                            ContentType = contentType
                        };

                        string minioKey = await _minioStorage.UploadEvidenceAsync(formFile);

                        stream.Close();
                        System.IO.File.Delete(localPath);

                        attendance.EvidenceObjectKey = minioKey;
                    }
                    catch (Exception ex)
                    {
                        TempData["AttendanceError"] = $"Không thể đẩy ảnh lên MinIO ({ex.Message}). Vui lòng kiểm tra container MinIO trong Docker.";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            var member = members[0];
            member.TrainingPoints += 1;

            attendance.Status = "Approved";
            attendance.TrainingPoints = 1;
            attendance.AdminNote = adminNote?.Trim() ?? "";

            await _context.SaveChangesAsync();

            TempData["AttendanceSuccess"] = $"Đã duyệt điểm danh cho {member.FullName}, cộng 1 điểm rèn luyện và lưu ảnh lên MinIO thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(
            int id,
            string? adminNote)
        {
            var attendance = await _context.ActivityAttendances
                .FirstOrDefaultAsync(x => x.AttendanceId == id);

            if (attendance == null)
            {
                return NotFound();
            }

            if (attendance.Status != "Pending")
            {
                TempData["AttendanceError"] = "Yêu cầu này đã được xử lý trước đó.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(adminNote))
            {
                TempData["AttendanceError"] = "Vui lòng nhập lý do từ chối.";
                return RedirectToAction(nameof(Index));
            }

            if (attendance.EvidenceObjectKey?.StartsWith("temp/") == true)
            {
                var relativePath = attendance.EvidenceObjectKey.Replace('/', Path.DirectorySeparatorChar);
                var localPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", relativePath);
                if (System.IO.File.Exists(localPath))
                {
                    System.IO.File.Delete(localPath);
                }
            }

            attendance.Status = "Rejected";
            attendance.TrainingPoints = 0;
            attendance.AdminNote = adminNote.Trim();

            await _context.SaveChangesAsync();

            TempData["AttendanceSuccess"] = "Đã từ chối yêu cầu điểm danh.";
            return RedirectToAction(nameof(Index));
        }

        private void LoadActivities()
        {
            ViewBag.Activities = new SelectList(
                _context.Activities
                    .AsNoTracking()
                    .ToList(),
                "ActivityId",
                "ActivityName");
        }
    }
}