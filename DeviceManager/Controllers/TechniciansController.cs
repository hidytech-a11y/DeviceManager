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
    public class TechniciansController : Controller
    {
        private readonly DeviceContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TechniciansController(DeviceContext context, UserManager<IdentityUser> userManager)
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

        /* ---------------- DELETED ---------------- */



        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deleted()
        {
            var technicians = await _context.Technicians
                .IgnoreQueryFilters()
                .Where(t => t.IsDeleted)
                .OrderBy(t => t.FullName)
                .AsNoTracking()
                .ToListAsync();

            return View(technicians);
        }

        /* ---------------- RESTORE (POST) ---------------- */

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(RestoreTechnicianViewModel model)
        {
            var tech = await _context.Technicians
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Id == model.TechnicianId);

            if (tech == null) return NotFound();

            tech.IsDeleted = false;
            tech.DeletedAt = null;

            if (model.SelectedDeviceIds.Any())
            {
                var devices = await _context.Devices
                    .Where(d => model.SelectedDeviceIds.Contains(d.Id))
                    .ToListAsync();

                foreach (var d in devices)
                {
                    d.TechnicianId = tech.Id;
                    d.Status = "Active";
                    d.WorkStatus = "Assigned";
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        /* ---------------- RESTORE (GET) ---------------- */



        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RestoreConfirm(int id)
        {
            var tech = await _context.Technicians
                .IgnoreQueryFilters()
                .Include(t => t.Devices)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tech == null) return NotFound();

            return View(tech);
        }



        /* ---------------- MY TASKS ---------------- */

        [Authorize(Roles = "Admin,Manager,Technician")]
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

        [Authorize(Roles ="technician")]
        [HttpPost]

        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var userId = _userManager.GetUserId(User);

            var technician = await _context.Technicians
                .FirstOrDefaultAsync(t => t.IdentityUserId == userId);

            if (technician == null)
                return Unauthorized();

            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.Id == id && d.TechnicianId == technician.Id);

            if (device == null)
                return NotFound();

            device.WorkStatus = status;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyTasks));

        }
    }
}
