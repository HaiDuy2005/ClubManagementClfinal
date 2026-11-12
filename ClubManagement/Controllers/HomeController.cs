using ClubManagement.Data;
using ClubManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ClubManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.TotalMembers = _context.Members.Count();
            ViewBag.TotalActivities = _context.Activities.Count();
            ViewBag.TotalClubs = 1;

            return View();
        }

        public async Task<IActionResult> Activities(
            string? searchName,
            string? searchLocation,
            DateTime? searchDate)
        {
            var query = _context.Activities.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                query = query.Where(a =>
                    a.ActivityName.Contains(searchName));
            }

            if (!string.IsNullOrWhiteSpace(searchLocation))
            {
                query = query.Where(a =>
                    a.Location.Contains(searchLocation));
            }

            if (searchDate.HasValue)
            {
                var date = searchDate.Value.Date;
                var nextDate = date.AddDays(1);

                query = query.Where(a =>
                    a.ActivityDate >= date &&
                    a.ActivityDate < nextDate);
            }

            var activities = await query
                .OrderByDescending(a => a.ActivityDate)
                .ThenByDescending(a => a.ActivityId)
                .ToListAsync();

            ViewBag.SearchName = searchName;
            ViewBag.SearchLocation = searchLocation;
            ViewBag.SearchDate = searchDate?.ToString("yyyy-MM-dd");

            return View(activities);
        }

        // GET: /Home/ActivityDetails/5
        public async Task<IActionResult> ActivityDetails(int id)
        {
            var activity = await _context.Activities
                .Include(a => a.Club)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ActivityId == id);

            if (activity == null)
            {
                return NotFound("Không tìm thấy hoạt động yêu cầu.");
            }

            return View(activity);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ??
                            HttpContext.TraceIdentifier
            });
        }
    }
}