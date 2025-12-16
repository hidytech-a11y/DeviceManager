using DeviceManager.Data;
using DeviceManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DeviceManager.Controllers
{
    [Authorize(Roles = "Admin,Manager,Technician,Viewer")]
    public class TechnicianController : Controller
    {
        private readonly DeviceContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TechnicianController(DeviceContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /* ---------------- HELPERS ---------------- */

        private async Task LoadTechnicianUsers(string? selectedUserId = null, int? technicianId = null)
        {
            var linkedUserIds = await _context.Technicians
                .Where(t => !t.IsDeleted)
                .Select(t => t.IdentityUserId!)
                .ToListAsync();

            if (!string.IsNullOrEmpty(selectedUserId))
                linkedUserIds.Remove(selectedUserId);

            var users = await _userManager.GetUsersInRoleAsync("Technician");

            var list = users
                .Where(u => !linkedUserIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Email })
                .ToList();

            ViewBag.Users = new SelectList(list, "Id", "Email", selectedUserId);
        }

        /* ---------------- INDEX ---------------- */

        public async Task<IActionResult> Index()
        {
            var technicians = await _context.Technicians
                .Where(t => !t.IsDeleted)
                .ToListAsync();
            return View(technicians);

        }

        /* ---------------- CREATE ---------------- */

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            await LoadTechnicianUsers();
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Technician t)
        {
            if (string.IsNullOrEmpty(t.IdentityUserId))
                ModelState.AddModelError("IdentityUserId", "User selection is required");

            var exists = await _context.Technicians
                .AnyAsync(x => x.IdentityUserId == t.IdentityUserId);

            if (exists)
                ModelState.AddModelError("IdentityUserId", "User already linked");

            if (!ModelState.IsValid)
            {
                await LoadTechnicianUsers(t.IdentityUserId);
                return View(t);
            }

            _context.Technicians.Add(t);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /* ---------------- EDIT ---------------- */

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var tech = await _context.Technicians.FindAsync(id);
            if (tech == null) return NotFound();

            await LoadTechnicianUsers(tech.IdentityUserId, tech.Id);
            return View(tech);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Technician t)
        {
            if (string.IsNullOrEmpty(t.IdentityUserId))
                ModelState.AddModelError("IdentityUserId", "User selection is required");

            var exists = await _context.Technicians
                .AnyAsync(x => x.IdentityUserId == t.IdentityUserId && x.Id != t.Id);

            if (exists)
                ModelState.AddModelError("IdentityUserId", "User already linked");

            if (!ModelState.IsValid)
            {
                await LoadTechnicianUsers(t.IdentityUserId, t.Id);
                return View(t);
            }

            var tech = await _context.Technicians.FindAsync(t.Id);
            if (tech == null) return NotFound();

            tech.FullName = t.FullName;
            tech.Phone = t.Phone;
            tech.Expertise = t.Expertise;
            tech.IdentityUserId = t.IdentityUserId;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /* ---------------- DETAILS ---------------- */

        [Authorize(Roles = "Admin,Manager,Viewer")]
        public async Task<IActionResult> Details(int id)
        {
            var tech = await _context.Technicians.FirstOrDefaultAsync(x => x.Id == id);
            if (tech == null) return NotFound();
            return View(tech);
        }

        /* ---------------- DELETE ---------------- */

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var tech = await _context.Technicians.FindAsync(id);
            if (tech == null) return NotFound();
            return View(tech);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tech = await _context.Technicians
                .Include(t => t.Devices)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tech == null) return NotFound();

            foreach (var d in tech.Devices)
            {
                d.TechnicianId = null;
                d.Status = "Inactive";
            }

            tech.IdentityUserId = null;
            tech.IsDeleted = true;
            tech.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /* ---------------- RESTORE ---------------- */

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Restore(int id)
        {
            var tech = await _context.Technicians
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tech == null) return NotFound();

            tech.IsDeleted = false;
            tech.DeletedAt = null;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /* ---------------- MY TASKS ---------------- */

        [Authorize(Roles = "Admin,Technician")]
        public async Task<IActionResult> MyTasks()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var tech = await _context.Technicians
                .FirstOrDefaultAsync(t => t.IdentityUserId == userId && !t.IsDeleted);

            if (tech == null) return Forbid();

            var devices = await _context.Devices
                .Include(d => d.DeviceType)
                .Where(d => d.TechnicianId == tech.Id)
                .ToListAsync();

            return View(devices);
        }
    }
}
