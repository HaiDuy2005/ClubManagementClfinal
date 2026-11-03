
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
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

    // POST: CLUBS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var club = await _context.Clubs.FindAsync(id);
        if (club == null)
        {
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _context.Clubs.Remove(club);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // CLB còn thành viên hoặc hoạt động liên quan nên chưa thể xóa.
            ModelState.AddModelError(string.Empty,
                "Không thể xóa câu lạc bộ này vì vẫn còn thành viên hoặc hoạt động thuộc câu lạc bộ. Hãy xóa hoặc chuyển các mục đó trước.");
            _context.ChangeTracker.Clear();
            var again = await _context.Clubs.FirstOrDefaultAsync(m => m.ClubId == id);
            return View(again);
        }

        return RedirectToAction(nameof(Index));
    }

    private bool ClubExists(int? id)
    {
        return _context.Clubs.Any(e => e.ClubId == id);
    }
}
