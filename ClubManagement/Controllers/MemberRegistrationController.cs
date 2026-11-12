using System;
using System.Linq;
using ClubManagement.Data;
using ClubManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClubManagement.Controllers
{
    public class MemberRegistrationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MemberRegistrationController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        private void LoadClubs(int? selectedId = null)
        {
            var clubs = _context.Clubs
                .AsNoTracking()
                .OrderBy(c => c.ClubName)
                .ToList();

            ViewBag.Clubs = new SelectList(clubs, "ClubId", "ClubName", selectedId);
        }

        [HttpGet]
        public IActionResult Create()
        {
            LoadClubs();
            return View(new MemberRegistration());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MemberRegistration registration)
        {
            registration.FullName = registration.FullName?.Trim() ?? "";
            registration.StudentCode = registration.StudentCode?.Trim() ?? "";
            registration.ClassName = registration.ClassName?.Trim() ?? "";
            registration.Phone = registration.Phone?.Trim() ?? "";
            registration.Email = registration.Email?.Trim() ?? "";
            registration.Reason = registration.Reason?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(registration.FullName))
            {
                ModelState.AddModelError(
                    nameof(registration.FullName),
                    "Vui lòng nhập họ tên.");
            }

            if (string.IsNullOrWhiteSpace(registration.StudentCode))
            {
                ModelState.AddModelError(
                    nameof(registration.StudentCode),
                    "Vui lòng nhập mã số sinh viên.");
            }

            if (string.IsNullOrWhiteSpace(registration.ClassName))
            {
                ModelState.AddModelError(
                    nameof(registration.ClassName),
                    "Vui lòng nhập lớp.");
            }

            if (string.IsNullOrWhiteSpace(registration.Phone))
            {
                ModelState.AddModelError(
                    nameof(registration.Phone),
                    "Vui lòng nhập số điện thoại.");
            }

            if (string.IsNullOrWhiteSpace(registration.Email))
            {
                ModelState.AddModelError(
                    nameof(registration.Email),
                    "Vui lòng nhập email.");
            }

            if (registration.ClubId == null || registration.ClubId <= 0)
            {
                ModelState.AddModelError(
                    nameof(registration.ClubId),
                    "Vui lòng chọn nhóm muốn tham gia.");
            }

            if (string.IsNullOrWhiteSpace(registration.Reason))
            {
                ModelState.AddModelError(
                    nameof(registration.Reason),
                    "Vui lòng nhập lý do tham gia nhóm.");
            }

            if (!ModelState.IsValid)
            {
                LoadClubs(registration.ClubId);
                return View(registration);
            }

            registration.CreatedAt = DateTime.Now;
            registration.Status = "Pending";

            try
            {
                _context.MemberRegistrations.Add(registration);
                _context.SaveChanges();

                TempData["Success"] =
                    "Đăng ký thành công! Vui lòng chờ Admin duyệt.";

                return RedirectToAction(nameof(Create));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "Không thể lưu đăng ký vào cơ sở dữ liệu. " +
                    "Vui lòng kiểm tra dữ liệu và thử lại.");

                LoadClubs(registration.ClubId);
                return View(registration);
            }
        }

        [HttpGet]
        public IActionResult AdminList(
            string? searchName,
            string? searchStudentCode,
            string? searchClass,
            string? searchEmail,
            string? searchStatus)
        {
            var query = _context.MemberRegistrations
                .Include(x => x.Club)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                query = query.Where(x =>
                    x.FullName.Contains(searchName));
            }

            if (!string.IsNullOrWhiteSpace(searchStudentCode))
            {
                query = query.Where(x =>
                    x.StudentCode.Contains(searchStudentCode));
            }

            if (!string.IsNullOrWhiteSpace(searchClass))
            {
                query = query.Where(x =>
                    x.ClassName.Contains(searchClass));
            }

            if (!string.IsNullOrWhiteSpace(searchEmail))
            {
                query = query.Where(x =>
                    x.Email.Contains(searchEmail));
            }

            if (!string.IsNullOrWhiteSpace(searchStatus) &&
                searchStatus != "All")
            {
                query = query.Where(x =>
                    x.Status == searchStatus);
            }

            var list = query
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            ViewBag.SearchName = searchName;
            ViewBag.SearchStudentCode = searchStudentCode;
            ViewBag.SearchClass = searchClass;
            ViewBag.SearchEmail = searchEmail;
            ViewBag.SearchStatus = searchStatus ?? "All";

            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id)
        {
            var registration = _context.MemberRegistrations
                .FirstOrDefault(x => x.RegistrationId == id);

            if (registration == null)
            {
                TempData["AdminError"] =
                    "Không tìm thấy yêu cầu đăng ký.";

                return RedirectToAction(nameof(AdminList));
            }

            if (registration.Status != "Pending")
            {
                TempData["AdminError"] =
                    "Yêu cầu này đã được xử lý trước đó.";

                return RedirectToAction(nameof(AdminList));
            }

            bool memberExists = _context.Members.Any(m =>
                m.StudentCode == registration.StudentCode);

            if (memberExists)
            {
                TempData["AdminError"] =
                    "Mã sinh viên này đã có trong danh sách thành viên.";

                return RedirectToAction(nameof(AdminList));
            }

            var targetClub = registration.ClubId.HasValue
                ? _context.Clubs.FirstOrDefault(c => c.ClubId == registration.ClubId.Value)
                : _context.Clubs.FirstOrDefault();

            if (targetClub == null)
            {
                TempData["AdminError"] =
                    "Nhóm được chọn không còn tồn tại trong hệ thống.";

                return RedirectToAction(nameof(AdminList));
            }

            var member = new Member
            {
                FullName = registration.FullName,
                StudentCode = registration.StudentCode,
                ClassName = registration.ClassName,
                Email = registration.Email,
                Phone = registration.Phone,
                Position = "Thành viên",
                TrainingPoints = 0,
                ClubId = targetClub.ClubId
            };

            _context.Members.Add(member);
            registration.Status = "Approved";

            try
            {
                _context.SaveChanges();

                TempData["AdminSuccess"] =
                    $"Đã duyệt đăng ký và thêm thành viên vào {targetClub.ClubName} thành công.";
            }
            catch (DbUpdateException)
            {
                _context.Entry(member).State = EntityState.Detached;
                registration.Status = "Pending";

                TempData["AdminError"] =
                    "Không thể lưu thành viên. Vui lòng kiểm tra lại dữ liệu.";
            }

            return RedirectToAction(nameof(AdminList));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int id)
        {
            var registration = _context.MemberRegistrations
                .FirstOrDefault(x => x.RegistrationId == id);

            if (registration == null)
            {
                TempData["AdminError"] =
                    "Không tìm thấy yêu cầu đăng ký.";

                return RedirectToAction(nameof(AdminList));
            }

            if (registration.Status != "Pending")
            {
                TempData["AdminError"] =
                    "Yêu cầu này đã được xử lý trước đó.";

                return RedirectToAction(nameof(AdminList));
            }

            registration.Status = "Rejected";

            try
            {
                _context.SaveChanges();

                TempData["AdminSuccess"] =
                    "Đã từ chối yêu cầu đăng ký.";
            }
            catch (DbUpdateException)
            {
                TempData["AdminError"] =
                    "Không thể cập nhật trạng thái đăng ký.";
            }

            return RedirectToAction(nameof(AdminList));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var registration = _context.MemberRegistrations
                .FirstOrDefault(x => x.RegistrationId == id);

            if (registration == null)
            {
                TempData["AdminError"] =
                    "Không tìm thấy đơn đăng ký.";

                return RedirectToAction(nameof(AdminList));
            }

            try
            {
                _context.MemberRegistrations.Remove(registration);
                _context.SaveChanges();

                TempData["AdminSuccess"] =
                    "Đã xóa đơn đăng ký thành công.";
            }
            catch (DbUpdateException)
            {
                TempData["AdminError"] =
                    "Không thể xóa đơn đăng ký này.";
            }

            return RedirectToAction(nameof(AdminList));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAll()
        {
            var allRegistrations = _context.MemberRegistrations.ToList();

            if (!allRegistrations.Any())
            {
                TempData["AdminError"] =
                    "Danh sách đơn đăng ký hiện đang trống.";

                return RedirectToAction(nameof(AdminList));
            }

            try
            {
                _context.MemberRegistrations.RemoveRange(allRegistrations);
                _context.SaveChanges();

                TempData["AdminSuccess"] =
                    $"Đã xóa toàn bộ {allRegistrations.Count} đơn đăng ký thành công.";
            }
            catch (DbUpdateException)
            {
                TempData["AdminError"] =
                    "Không thể xóa toàn bộ danh sách đơn đăng ký.";
            }

            return RedirectToAction(nameof(AdminList));
        }
    }
}