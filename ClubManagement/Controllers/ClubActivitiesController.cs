using System;
using System.Linq;
using System.Threading.Tasks;
using ClubManagement.Models;
using ClubManagement.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClubManagement.Controllers
{
    public class ClubActivitiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClubActivitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string? searchName,
            string? searchLocation,
            DateTime? searchDate)
        {
            var query = _context.Activities
                .AsNoTracking()
                .AsQueryable();

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

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var activity = await _context.Activities
                .FirstOrDefaultAsync(a => a.ActivityId == id);

            if (activity == null)
                return NotFound();

            return View(activity);
        }

        [HttpGet]
        public async Task<IActionResult> PublicDetails(int? id)
        {
            if (id == null)
                return NotFound();

            var activity = await _context.Activities
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ActivityId == id);

            if (activity == null)
                return NotFound();

            return View(activity);
        }

        public async Task<IActionResult> Create()
        {
            await LoadClubsAsync(null);

            return View(new ClubActivity
            {
                ActivityDate = DateTime.Now
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("ActivityId,ActivityName,Description,Location,ActivityDate,ClubId")]
            ClubActivity clubactivity)
        {
            if (ModelState.IsValid)
            {
                _context.Add(clubactivity);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            await LoadClubsAsync(clubactivity.ClubId);

            return View(clubactivity);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var activity = await _context.Activities.FindAsync(id);

            if (activity == null)
                return NotFound();

            await LoadClubsAsync(activity.ClubId);

            return View(activity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("ActivityId,ActivityName,Description,Location,ActivityDate,ClubId")]
            ClubActivity clubactivity)
        {
            if (id != clubactivity.ActivityId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(clubactivity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClubActivityExists(clubactivity.ActivityId))
                        return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            await LoadClubsAsync(clubactivity.ClubId);

            return View(clubactivity);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var activity = await _context.Activities
                .FirstOrDefaultAsync(a => a.ActivityId == id);

            if (activity == null)
                return NotFound();

            return View(activity);
        }

        // POST: ClubActivities/Delete/5 (XÓA DÂY CHUYỀN HOẠT ĐỘNG TỰ ĐỘNG)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var activity = await _context.Activities.FindAsync(id);

            if (activity == null)
                return RedirectToAction(nameof(Index));

            // 1. Xóa các đơn đăng ký thuộc về hoạt động này
            var registrations = _context.ActivityRegistrations
                .Where(r => r.ActivityId == id);
            _context.ActivityRegistrations.RemoveRange(registrations);

            // 2. Xóa dữ liệu điểm danh thuộc về hoạt động này
            var attendances = _context.Set<Attendance>()
                .Where(a => a.ActivityId == id);
            _context.Set<Attendance>().RemoveRange(attendances);

            // 3. Xóa hoạt động (Không làm ảnh hưởng Nhóm hay Thành viên)
            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadClubsAsync(int? selectedId)
        {
            var clubs = await _context.Clubs
                .AsNoTracking()
                .OrderBy(c => c.ClubName)
                .ToListAsync();

            ViewBag.Clubs = new SelectList(
                clubs,
                "ClubId",
                "ClubName",
                selectedId);
        }

        private bool ClubActivityExists(int id)
        {
            return _context.Activities.Any(a =>
                a.ActivityId == id);
        }

        public async Task<IActionResult> Participants(int id)
        {
            var list = await _context.ActivityRegistrations
                .AsNoTracking()
                .Where(x =>
                    x.ActivityId == id &&
                    x.IsApproved == true)
                .ToListAsync();

            return View(list);
        }
    }
}