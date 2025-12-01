using DeviceManager.Data;
using DeviceManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DeviceManager.Controllers
{
    public class DevicesController(DeviceContext context, ILogger<DevicesController> logger) : Controller
    {
        private readonly DeviceContext _context = context;
        private readonly ILogger<DevicesController> _logger = logger;

        // LIST
        public async Task<IActionResult> Index()
        {
            var devices = await _context.Devices
                .Include(d => d.Technician)
                .Include(d => d.DeviceType)
                .ToListAsync();

            return View(devices);
        }

        // CREATE GET
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        // CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Device device)
        {
            // Important fix: Remove Required from manual "Type"
            ModelState.Remove("Type");

            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                LoadDropdowns();
                return View(device);
            }

            device.DeviceTypeId = device.DeviceTypeId == 0 ? null : device.DeviceTypeId;
            device.TechnicianId = device.TechnicianId == 0 ? null : device.TechnicianId;

            _context.Devices.Add(device);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Save error");
                ModelState.AddModelError("", "Error saving device");
                LoadDropdowns();
                return View(device);
            }

            return RedirectToAction(nameof(Index));
        }

        // EDIT GET
        public async Task<IActionResult> Edit(int id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null)
                return NotFound();

            LoadDropdowns(device.TechnicianId, device.DeviceTypeId);
            return View(device);
        }

        // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Device device)
        {
            ModelState.Remove("Type");

            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                LoadDropdowns(device.TechnicianId, device.DeviceTypeId);
                return View(device);
            }

            var existing = await _context.Devices.FindAsync(device.Id);
            if (existing == null)
                return NotFound();

            existing.Name = device.Name;
            existing.SerialNumber = device.SerialNumber;
            existing.Status = device.Status;

            existing.DeviceTypeId = device.DeviceTypeId == 0 ? null : device.DeviceTypeId;
            existing.TechnicianId = device.TechnicianId == 0 ? null : device.TechnicianId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Update error");
                ModelState.AddModelError("", "Error updating device");
                LoadDropdowns(device.TechnicianId, device.DeviceTypeId);
                return View(device);
            }

            return RedirectToAction(nameof(Index));
        }

        // DETAILS
        public async Task<IActionResult> Details(int id)
        {
            var device = await _context.Devices
                .Include(d => d.Technician)
                .Include(d => d.DeviceType)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (device == null)
                return NotFound();

            return View(device);
        }

        // DELETE GET
        public async Task<IActionResult> Delete(int id)
        {
            var device = await _context.Devices
                .Include(d => d.Technician)
                .Include(d => d.DeviceType)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (device == null)
                return NotFound();

            return View(device);
        }

        // DELETE POST
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
            return RedirectToAction(nameof(Index));
        }

        // HELPERS
        private void LoadDropdowns(int? selectedTech = null, int? selectedType = null)
        {
            ViewBag.Technicians = new SelectList(
                _context.Technicians.OrderBy(t => t.FullName),
                "Id",
                "FullName",
                selectedTech
            );

            ViewBag.DeviceTypes = new SelectList(
                _context.DeviceTypes.OrderBy(t => t.Name),
                "Id",
                "Name",
                selectedType
            );
        }

        private void LogModelStateErrors()
        {
            foreach (var kvp in ModelState)
            {
                foreach (var error in kvp.Value.Errors)
                {
                    _logger.LogWarning("ModelState error. Field: {Field}, Error: {Error}",
                        kvp.Key, error.ErrorMessage);
                }
            }
        }
    }
}
