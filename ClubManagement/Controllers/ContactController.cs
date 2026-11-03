
using System;
using System.Linq;
using ClubManagement.Data;
using ClubManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClubManagement.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Contact contact)
        {
            if (contact.IsAnonymous)
            {
                contact.Name = "Ẩn Danh";
            }

            contact.CreatedAt = DateTime.Now;

            _context.Contacts.Add(contact);
            _context.SaveChanges();

            TempData["Success"] = "Gửi liên hệ thành công!";

            return RedirectToAction(nameof(Create));
        }

        [HttpGet]
        public IActionResult AdminList(
            string? searchName,
            string? searchEmail,
            string? searchMessage,
            DateTime? searchDate)
        {
            var query = _context.Contacts
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                query = query.Where(x =>
                    x.Name.Contains(searchName));
            }

            if (!string.IsNullOrWhiteSpace(searchEmail))
            {
                query = query.Where(x =>
                    x.Email.Contains(searchEmail));
            }

            if (!string.IsNullOrWhiteSpace(searchMessage))
            {
                query = query.Where(x =>
                    x.Message.Contains(searchMessage));
            }

            if (searchDate.HasValue)
            {
                var date = searchDate.Value.Date;
                var nextDate = date.AddDays(1);

                query = query.Where(x =>
                    x.CreatedAt >= date &&
                    x.CreatedAt < nextDate);
            }

            var contacts = query
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            ViewBag.SearchName = searchName;
            ViewBag.SearchEmail = searchEmail;
            ViewBag.SearchMessage = searchMessage;
            ViewBag.SearchDate = searchDate?.ToString("yyyy-MM-dd");

            return View(contacts);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var contact = _context.Contacts.Find(id);

            if (contact == null)
            {
                TempData["Error"] = "Không tìm thấy liên hệ cần xóa!";
                return RedirectToAction(nameof(AdminList));
            }

            _context.Contacts.Remove(contact);
            _context.SaveChanges();

            TempData["Success"] = "Xóa liên hệ thành công!";

            return RedirectToAction(nameof(AdminList));
        }
    }
}
