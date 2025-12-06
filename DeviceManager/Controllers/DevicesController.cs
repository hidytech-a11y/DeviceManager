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
        private const int PageSize = 10;

        public async Task<IActionResult> Index(
         string search,
         string sortOrder,
         string typeFilter,
         string statusFilter,
         int? technicianId,
         int pageNumber = 1)
        {
            var query = _context.Devices
                .Include(d => d.DeviceType)
                .Include(d => d.Technician)
                .AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(d =>
                    d.Name.Contains(search) ||
                    d.SerialNumber.Contains(search));
            }

            // Filters
            if (!string.IsNullOrEmpty(typeFilter))
                query = query.Where(d => d.DeviceType != null && d.DeviceType.Name == typeFilter);

            if (!string.IsNullOrEmpty(statusFilter))
                query = query.Where(d => d.Status == statusFilter);

            if (technicianId.HasValue)
                query = query.Where(d => d.TechnicianId == technicianId.Value);

            // Sorting
            query = sortOrder switch
            {
                "name_desc" => query.OrderByDescending(d => d.Name),
                "type" => query.OrderBy(d => d.DeviceType != null ? d.DeviceType.Name : string.Empty),
                "type_desc" => query.OrderByDescending(d => d.DeviceType != null ? d.DeviceType.Name : string.Empty),
                _ => query.OrderBy(d => d.Name)
            };

            // Pagination
            int totalDevices = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Build ViewModel
            var vm = new DeviceListViewModel
            {
                Devices = items,
                PageNumber = pageNumber,
                TotalPages = (int)Math.Ceiling(totalDevices / (double)PageSize),

                Search = search,
                SortOrder = sortOrder,
                TypeFilter = typeFilter,
                StatusFilter = statusFilter,
                TechnicianId = technicianId,

                // Stats
                TotalDevices = await _context.Devices.CountAsync(),
                ActiveCount = await _context.Devices.CountAsync(d => d.Status == "Active"),
                InactiveCount = await _context.Devices.CountAsync(d => d.Status == "Inactive")
            };

            return View(vm);
        }


        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Device device)
        {
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

        public async Task<IActionResult> Edit(int id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null)
                return NotFound();

            LoadDropdowns(device.TechnicianId, device.DeviceTypeId);
            return View(device);
        }

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
