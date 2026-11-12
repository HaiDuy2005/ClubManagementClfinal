using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClubManagement.Models;
using ClubManagement.Data;

public class ClubsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ClubsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: CLUBS
    public async Task<IActionResult> Index()
    {
        return View(await _context.Clubs.ToListAsync());
    }

    // GET: CLUBS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var club = await _context.Clubs
            .FirstOrDefaultAsync(m => m.ClubId == id);
        if (club == null)
        {
            return NotFound();
        }

        return View(club);
    }

    // GET: CLUBS/Create
    public IActionResult Create()
    {
        return View(new Club { CreatedDate = DateTime.Now });
    }

    // POST: CLUBS/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ClubId,ClubName,Description,CreatedDate")] Club club)
    {
        if (ModelState.IsValid)
        {
            _context.Add(club);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(club);
    }

    // GET: CLUBS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var club = await _context.Clubs.FindAsync(id);
        if (club == null)
        {
            return NotFound();
        }
        return View(club);
    }

    // POST: CLUBS/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("ClubId,ClubName,Description,CreatedDate")] Club club)
    {
        if (id != club.ClubId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(club);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClubExists(club.ClubId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(club);
    }

    // GET: CLUBS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var club = await _context.Clubs
            .FirstOrDefaultAsync(m => m.ClubId == id);
        if (club == null)
        {
            return NotFound();
        }

        return View(club);
    }

    // POST: CLUBS/Delete/5 (XÓA DÂY CHUYỀN TỰ ĐỘNG)
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var club = await _context.Clubs
            .Include(c => c.Members)
            .Include(c => c.Activities)
            .FirstOrDefaultAsync(m => m.ClubId == id);

        if (club != null)
        {
            // 1. Tự động xóa tất cả hoạt động, đăng ký và điểm danh của nhóm
            if (club.Activities != null && club.Activities.Any())
            {
                foreach (var activity in club.Activities.ToList())
                {
                    var registrations = _context.ActivityRegistrations
                        .Where(r => r.ActivityId == activity.ActivityId);
                    _context.ActivityRegistrations.RemoveRange(registrations);

                    var attendances = _context.ActivityAttendances
                        .Where(a => a.ActivityId == activity.ActivityId);
                    _context.ActivityAttendances.RemoveRange(attendances);

                    _context.Activities.Remove(activity);
                }
            }

            // 2. Tự động xóa tất cả thành viên thuộc nhóm
            if (club.Members != null && club.Members.Any())
            {
                _context.Members.RemoveRange(club.Members);
            }

            // 3. Xóa chính nhóm này
            _context.Clubs.Remove(club);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool ClubExists(int? id)
    {
        return _context.Clubs.Any(e => e.ClubId == id);
    }
}