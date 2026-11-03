using System;
using System.Linq;
using System.Threading.Tasks;
using ClubManagement.Data;
using ClubManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClubManagement.Controllers
{
    public class MembersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MembersController(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index(
            string? searchName,
            string? searchStudentCode,
            string? searchClass,
            string? searchEmail,
            string? searchPhone,
            string? searchPosition)
        {
            var query = _context.Members
                .AsNoTracking()
                .AsQueryable();


            if (!string.IsNullOrWhiteSpace(searchName))
            {
                query = query.Where(m =>
                    m.FullName.Contains(searchName));
            }


            if (!string.IsNullOrWhiteSpace(searchStudentCode))
            {
                query = query.Where(m =>
                    m.StudentCode.Contains(searchStudentCode));
            }


            if (!string.IsNullOrWhiteSpace(searchClass))
            {
                query = query.Where(m =>
                    m.ClassName.Contains(searchClass));
            }


            if (!string.IsNullOrWhiteSpace(searchEmail))
            {
                query = query.Where(m =>
                    m.Email.Contains(searchEmail));
            }


            if (!string.IsNullOrWhiteSpace(searchPhone))
            {
                query = query.Where(m =>
                    m.Phone.Contains(searchPhone));
            }


            if (!string.IsNullOrWhiteSpace(searchPosition))
            {
                query = query.Where(m =>
                    m.Position.Contains(searchPosition));
            }

            var members = await query
                .OrderBy(m => m.FullName)
                .ToListAsync();


            ViewBag.SearchName = searchName;
            ViewBag.SearchStudentCode = searchStudentCode;
            ViewBag.SearchClass = searchClass;
            ViewBag.SearchEmail = searchEmail;
            ViewBag.SearchPhone = searchPhone;
            ViewBag.SearchPosition = searchPosition;

            return View(members);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.MemberId == id);

            if (member == null)
                return NotFound();

            return View(member);
        }


        public async Task<IActionResult> Create()
        {
            await LoadClubsAsync(null);
            return View(new Member());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("MemberId,FullName,StudentCode,ClassName,Email,Phone,Position,TrainingPoints,ClubId")]
            Member member)
        {
            if (ModelState.IsValid)
            {
                _context.Add(member);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            await LoadClubsAsync(member.ClubId);
            return View(member);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var member = await _context.Members.FindAsync(id);

            if (member == null)
                return NotFound();

            await LoadClubsAsync(member.ClubId);
            return View(member);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MemberId,FullName,StudentCode,ClassName,Email,Phone,Position,TrainingPoints,ClubId")]
            Member member)
        {
            if (id != member.MemberId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(member);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberExists(member.MemberId))
                        return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            await LoadClubsAsync(member.ClubId);
            return View(member);
        }


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.MemberId == id);

            if (member == null)
                return NotFound();

            return View(member);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var member = await _context.Members.FindAsync(id);

            if (member == null)
                return RedirectToAction(nameof(Index));

            try
            {
                _context.Members.Remove(member);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty,
                    "Không thể xóa thành viên này vì vẫn còn dữ liệu điểm danh liên quan.");
                _context.ChangeTracker.Clear();
                var again = await _context.Members
                    .FirstOrDefaultAsync(m => m.MemberId == id);
                return View(again);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadClubsAsync(int? selectedId)
        {
            var clubs = await _context.Clubs
                .AsNoTracking()
                .OrderBy(c => c.ClubName)
                .ToListAsync();

            ViewBag.Clubs = new SelectList(clubs, "ClubId", "ClubName", selectedId);
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(m => m.MemberId == id);
        }
    }
}