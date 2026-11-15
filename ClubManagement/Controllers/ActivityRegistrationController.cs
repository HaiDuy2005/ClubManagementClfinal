using System;
using System.Linq;
using ClubManagement.Data;
using ClubManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClubManagement.Controllers
{
    public class ActivityRegistrationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActivityRegistrationController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create(int id)
        {
            var activity = _context.Activities
                .FirstOrDefault(x => x.ActivityId == id);

            if (activity == null)
                return NotFound();

            ViewBag.Clubs = _context.Clubs
                .OrderBy(c => c.ClubName)
                .ToList();

            var model = new ActivityRegistration
            {
                ActivityId = activity.ActivityId,
                ActivityName = activity.ActivityName,
                ActivityDate = activity.ActivityDate
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ActivityRegistration registration)
        {
            registration.FullName = registration.FullName?.Trim() ?? string.Empty;
            registration.ClassName = registration.ClassName?.Trim() ?? string.Empty;
            registration.StudentCode = registration.StudentCode?.Trim() ?? string.Empty;
            registration.Email = registration.Email?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(registration.FullName) ||
                string.IsNullOrWhiteSpace(registration.ClassName) ||
                string.IsNullOrWhiteSpace(registration.StudentCode) ||
                string.IsNullOrWhiteSpace(registration.Email))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng điền đầy đủ họ tên, lớp, mã sinh viên và email.");
            }

            // Nếu sinh viên chọn có thuộc nhóm trong CLB -> Kiểm tra có trong danh sách thành viên của nhóm đó không
            if (registration.ClubId.HasValue && !string.IsNullOrWhiteSpace(registration.StudentCode))
            {
                bool isMemberInGroup = _context.Members.Any(m =>
                    m.ClubId == registration.ClubId.Value &&
                    m.StudentCode == registration.StudentCode);

                if (!isMemberInGroup)
                {
                    ModelState.AddModelError("ClubId",
                        "Không tìm thấy mã sinh viên này trong nhóm đã chọn. Nếu bạn là sinh viên ngoài CLB, vui lòng chọn 'Không thuộc CLB'.");
                }
            }
            // Nếu ClubId == null ("Không thuộc CLB") -> Không cần kiểm tra bảng Members

            if (!ModelState.IsValid)
            {
                ViewBag.Clubs = _context.Clubs
                    .OrderBy(c => c.ClubName)
                    .ToList();
                return View(registration);
            }

            registration.IsApproved = false;
            _context.ActivityRegistrations.Add(registration);
            _context.SaveChanges();

            TempData["Success"] = registration.ClubId.HasValue
                ? "Đăng ký hoạt động thành công! Điểm rèn luyện sẽ được cộng sau khi ban quản trị duyệt."
                : "Đăng ký tham gia hoạt động thành công! (Lưu ý: Bạn đăng ký với tư cách Không thuộc CLB nên sẽ không được cộng điểm rèn luyện).";

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Index(
            string? searchName,
            string? searchEmail,
            string? searchActivity,
            string? searchStatus)
        {
            var query = _context.ActivityRegistrations
                .Include(x => x.Club)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                query = query.Where(x => x.FullName.Contains(searchName));
            }

            if (!string.IsNullOrWhiteSpace(searchEmail))
            {
                query = query.Where(x => x.Email.Contains(searchEmail));
            }

            if (!string.IsNullOrWhiteSpace(searchActivity))
            {
                query = query.Where(x => x.ActivityName.Contains(searchActivity));
            }

            if (!string.IsNullOrWhiteSpace(searchStatus) && searchStatus != "All")
            {
                bool isApproved = searchStatus == "Approved";
                query = query.Where(x => x.IsApproved == isApproved);
            }

            var list = query
                .OrderByDescending(x => x.RegistrationId)
                .ToList();

            ViewBag.SearchName = searchName;
            ViewBag.SearchEmail = searchEmail;
            ViewBag.SearchActivity = searchActivity;
            ViewBag.SearchStatus = searchStatus ?? "All";

            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id)
        {
            var registration = _context.ActivityRegistrations
                .FirstOrDefault(x => x.RegistrationId == id);

            if (registration == null)
                return NotFound();

            if (registration.IsApproved)
            {
                TempData["AdminError"] = "Đăng ký này đã được duyệt trước đó.";
                return RedirectToAction(nameof(Index));
            }

            registration.IsApproved = true;

            // Chỉ đối chiếu danh sách thành viên và cộng điểm nếu đơn có chọn nhóm của CLB
            if (registration.ClubId.HasValue)
            {
                var member = _context.Members.FirstOrDefault(m =>
                    m.ClubId == registration.ClubId.Value &&
                    m.StudentCode == registration.StudentCode);

                if (member != null)
                {
                    member.TrainingPoints += 1;
                    TempData["AdminSuccess"] =
                        $"Đã duyệt đơn và cộng 1 điểm rèn luyện cho thành viên {member.FullName}.";
                }
                else
                {
                    TempData["AdminSuccess"] =
                        "Đã duyệt đăng ký hoạt động (Không tìm thấy hồ sơ thành viên trong nhóm để cộng điểm).";
                }
            }
            else
            {
                // Không thuộc CLB -> Bỏ qua kiểm tra Members, không cộng điểm
                TempData["AdminSuccess"] =
                    "Đã duyệt đơn tham gia hoạt động (Sinh viên ngoài CLB — không cộng điểm rèn luyện).";
            }

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException)
            {
                TempData["AdminError"] = "Không thể duyệt đăng ký. Vui lòng thử lại.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var registration = _context.ActivityRegistrations
                .FirstOrDefault(x => x.RegistrationId == id);

            if (registration == null)
            {
                TempData["AdminError"] = "Không tìm thấy đơn đăng ký cần xóa!";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.ActivityRegistrations.Remove(registration);
                _context.SaveChanges();
                TempData["AdminSuccess"] = "Đã xóa đơn đăng ký hoạt động thành công.";
            }
            catch (DbUpdateException)
            {
                TempData["AdminError"] =
                    "Không thể xóa đơn đăng ký. Dữ liệu có thể đang được liên kết với thông tin khác.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}