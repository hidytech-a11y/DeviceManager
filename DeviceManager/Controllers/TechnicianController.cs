using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeviceManager.Data;
using DeviceManager.Models;
using Microsoft.AspNetCore.Authorization;

namespace DeviceManager.Controllers
{
    [Authorize(Roles = "Admin,Manager,Viewer")]
    public class TechnicianController : Controller
    {
        private readonly DeviceContext _context;

        public TechnicianController(DeviceContext context)
        {
            _context = context;
        }

        // GET: Technician
        public async Task<IActionResult> Index()
        {
            var technicians = await _context.Technicians.ToListAsync();
            return View(technicians);
        }

        // GET: Technician/Create

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Technician/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Technician t)
        {
            if (!ModelState.IsValid)
                return View(t);

            _context.Technicians.Add(t);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Technician/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var tech = await _context.Technicians.FindAsync(id);
            if (tech == null) return NotFound();
            return View(tech);
        }

        // POST: Technician/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Technician t)
        {
            if (!ModelState.IsValid)
                return View(t);

            _context.Technicians.Update(t);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Technician/Details/5

        [Authorize(Roles = "Admin,Manager,Viewer")]
        public async Task<IActionResult> Details(int id)
        {
            var tech = await _context.Technicians
                .FirstOrDefaultAsync(x => x.Id == id);

            if (tech == null) return NotFound();
            return View(tech);
        }

        // GET: Technician/Delete/5
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var tech = await _context.Technicians.FindAsync(id);
            if (tech == null) return NotFound();
            return View(tech);
        }

        // POST: Technician/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tech = await _context.Technicians.FindAsync(id);
            if (tech != null)
            {
                _context.Technicians.Remove(tech);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
