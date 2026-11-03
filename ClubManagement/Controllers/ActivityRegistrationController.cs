
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
            _context.ActivityRegistrations.Add(registration);
            _context.SaveChanges();

            TempData["Success"] = "Đăng ký thành công!";

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
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                query = query.Where(x =>
                    x.FullName.Contains(searchName));
            }

            if (!string.IsNullOrWhiteSpace(searchEmail))
            {
                query = query.Where(x =>
                    x.Email.Contains(searchEmail));
            }

            if (!string.IsNullOrWhiteSpace(searchActivity))
            {
                query = query.Where(x =>
                    x.ActivityName.Contains(searchActivity));
            }

            if (!string.IsNullOrWhiteSpace(searchStatus) &&
                searchStatus != "All")
            {
                bool isApproved = searchStatus == "Approved";

                query = query.Where(x =>
                    x.IsApproved == isApproved);
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
                TempData["AdminError"] =
                    "Đăng ký này đã được duyệt trước đó.";

                return RedirectToAction(nameof(Index));
            }

            var member = new Member
            {
                FullName = registration.FullName,
                Email = registration.Email,
                Phone = "",
                Position = "Thành viên",
                ClubId = 1
            };

            registration.IsApproved = true;
            _context.Members.Add(member);

            try
            {
                _context.SaveChanges();

                TempData["AdminSuccess"] =
                    "Đã duyệt đăng ký hoạt động thành công.";
            }
            catch (DbUpdateException)
            {
                TempData["AdminError"] =
                    "Không thể duyệt đăng ký. Vui lòng kiểm tra dữ liệu thành viên và mã CLB.";

                return RedirectToAction(nameof(Index));
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
                TempData["AdminError"] =
                    "Không tìm thấy đơn đăng ký cần xóa!";

                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.ActivityRegistrations.Remove(registration);
                _context.SaveChanges();

                TempData["AdminSuccess"] =
                    "Đã xóa đơn đăng ký hoạt động thành công.";
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
