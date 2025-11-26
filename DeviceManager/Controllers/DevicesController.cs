using DeviceManager.Data;
using DeviceManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DeviceManager.Controllers
{
    public class DevicesController : Controller
    {
        private readonly DeviceContext _context;

        public DevicesController(DeviceContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var devices = await _context.Devices
                .Include(d => d.Technician)
                .ToListAsync();

            return View(devices);
        }

        public IActionResult Create()
        {
            ViewBag.Technicians = new SelectList(_context.Technicians.ToList(), "Id", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Device device)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Technicians = new SelectList(_context.Technicians.ToList(), "Id", "FullName");
                return View(device);
            }

            if (device.TechnicianId == 0)
                device.TechnicianId = null;

            _context.Devices.Add(device);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null) return NotFound();

            ViewBag.Technicians = new SelectList(_context.Technicians.ToList(), "Id", "FullName", device.TechnicianId);

            return View(device);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Device device)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Technicians = new SelectList(_context.Technicians.ToList(), "Id", "FullName", device.TechnicianId);
                return View(device);
            }

            var existing = await _context.Devices.FindAsync(device.Id);
            if (existing == null) return NotFound();

            existing.Name = device.Name;
            existing.Type = device.Type;
            existing.SerialNumber = device.SerialNumber;
            existing.Status = device.Status;
            existing.TechnicianId = device.TechnicianId;

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int id)
        {
            var device = await _context.Devices.Include(d => d.Technician).FirstOrDefaultAsync(d => d.Id == id);
            if (device == null) return NotFound();
            return View(device);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var device = await _context.Devices.Include(d => d.Technician).FirstOrDefaultAsync(d => d.Id == id);
            if (device == null) return NotFound();
            return View(device);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device != null)
            {
                _context.Devices.Remove(device);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}