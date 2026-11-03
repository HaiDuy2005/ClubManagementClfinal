using ClubManagement.Data;
using Microsoft.AspNetCore.Mvc;

namespace ClubManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var admin = _context.Admins
                .FirstOrDefault(x =>
                    x.Username == username &&
                    x.Password == password);

            if (admin != null)
            {
                HttpContext.Session.SetString("Admin", admin.Username);

                return RedirectToAction("Dashboard");
            }

            ViewBag.Error = "Sai tài khoản hoặc mật khẩu";

            return View();
        }

        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("Admin") == null)
            {
                return RedirectToAction("Login");
            }

            ViewBag.TotalMembers = _context.Members.Count();
            ViewBag.TotalActivities = _context.Activities.Count();
            ViewBag.TotalClubs = _context.Clubs.Count();

            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
    }
}