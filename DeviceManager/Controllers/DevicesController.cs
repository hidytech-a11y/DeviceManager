using DeviceManager.Data;
using DeviceManager.Models;
using DeviceManager.Services;
using DeviceManager.Services.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceManager.Controllers
{
    [Authorize]
    public class DevicesController : Controller
    {
        private readonly DeviceContext _context;
        private readonly ILogger<DevicesController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAuditService _audit;
        private const int PageSize = 10;

        public DevicesController(
            DeviceContext context,
            ILogger<DevicesController> logger,
            UserManager<IdentityUser> userManager,
            IAuditService audit)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _audit = audit;
        }

        // LIST
        [Authorize(Roles = "Admin,Technician,Viewer,Manager")]
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

            if (User.IsInRole("Technician"))
            {
                var user = await _userManager.GetUserAsync(User);
                var tech = await _context.Technicians
                    .FirstOrDefaultAsync(t => t.IdentityUserId == user.Id);

                query = tech == null
                    ? query.Where(d => false)
                    : query.Where(d => d.TechnicianId == tech.Id);
            }

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(d => d.Name.Contains(search) || d.SerialNumber.Contains(search));

            if (!string.IsNullOrWhiteSpace(typeFilter))
                query = query.Where(d => d.DeviceType != null && d.DeviceType.Name == typeFilter);

            if (!string.IsNullOrWhiteSpace(statusFilter))
                query = query.Where(d => d.Status == statusFilter);

            if (technicianId.HasValue)
                query = query.Where(d => d.TechnicianId == technicianId);

            query = sortOrder switch
            {
                "name_desc" => query.OrderByDescending(d => d.Name),
                "type" => query.OrderBy(d => d.DeviceType!.Name),
                "type_desc" => query.OrderByDescending(d => d.DeviceType!.Name),
                _ => query.OrderBy(d => d.Name)
            };

            var totalItems = await query.CountAsync();
            var devices = await query
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var vm = new DeviceListViewModel
            {
                Devices = devices,
                PageNumber = pageNumber,
                TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize),
                Search = search,
                SortOrder = sortOrder,
                TypeFilter = typeFilter,
                StatusFilter = statusFilter,
                TechnicianId = technicianId
            };

            ViewBag.AvailableTypes = await _context.DeviceTypes.ToListAsync();
            ViewBag.Technicians = await _context.Technicians.ToListAsync();

            return View(vm);
        }

        // CREATE
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Device device)
        {
            ModelState.Remove("Type");

            if (!ModelState.IsValid)
            {
                LoadDropdowns();
                return View(device);
            }

            device.DeviceTypeId = device.DeviceTypeId == 0 ? null : device.DeviceTypeId;
            device.TechnicianId = device.TechnicianId == 0 ? null : device.TechnicianId;

            if (device.TechnicianId != null)
            {
                device.WorkStatus = "Assigned";
                device.IsApprovedByManager = false;
            }

            _context.Devices.Add(device);
            await _context.SaveChangesAsync();

            await _audit.LogAsync(
                device.Id,
                "Device Created",
                null,
                device.Name,
                User.Identity.Name
            );

            return RedirectToAction(nameof(Index));
        }

        // EDIT
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null) return NotFound();

            LoadDropdowns(device.TechnicianId, device.DeviceTypeId);
            return View(device);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Device device)
        {
            var existing = await _context.Devices.FindAsync(device.Id);
            if (existing == null) return NotFound();

            var oldStatus = existing.Status;
            var oldTech = existing.TechnicianId;

            existing.Name = device.Name;
            existing.SerialNumber = device.SerialNumber;
            existing.Status = device.Status;
            existing.DeviceTypeId = device.DeviceTypeId == 0 ? null : device.DeviceTypeId;

            if (oldTech != device.TechnicianId)
            {
                existing.TechnicianId = device.TechnicianId == 0 ? null : device.TechnicianId;
                existing.WorkStatus = existing.TechnicianId == null ? null : "Assigned";
                existing.IsApprovedByManager = false;

                await _audit.LogAsync(
                    existing.Id,
                    "Technician Changed",
                    oldTech?.ToString(),
                    existing.TechnicianId?.ToString(),
                    User.Identity.Name
                );
            }

            if (oldStatus != existing.Status)
            {
                await _audit.LogAsync(
                    existing.Id,
                    "Status Changed",
                    oldStatus,
                    existing.Status,
                    User.Identity.Name
                );
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // DETAILS
        [Authorize(Roles = "Admin,Manager,Technician,Viewer")]
        public async Task<IActionResult> Details(int id)
        {
            var device = await _context.Devices
                .Include(d => d.DeviceType)
                .Include(d => d.Technician)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (device == null) return NotFound();
            return View(device);
        }

        // TECHNICIAN STATUS UPDATE
        [Authorize(Roles = "Technician")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null) return NotFound();

            var old = device.WorkStatus;
            device.WorkStatus = status;

            await _context.SaveChangesAsync();

            await _audit.LogAsync(
                device.Id,
                "Work Status Updated",
                old,
                status,
                User.Identity.Name
            );

            return RedirectToAction("MyTasks", "Technician");
        }

        // MANAGER APPROVAL LIST
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> PendingApproval()
        {
            var devices = await _context.Devices
                .Include(d => d.Technician)
                .Where(d => d.WorkStatus == "Done" && !d.IsApprovedByManager)
                .ToListAsync();

            return View(devices);
        }

        // MANAGER APPROVE
        [Authorize(Roles = "Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null) return NotFound();

            device.IsApprovedByManager = true;
            device.ApprovedByManagerId = _userManager.GetUserId(User);
            device.ApprovedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _audit.LogAsync(
                device.Id,
                "Manager Approval",
                "Pending",
                "Approved",
                User.Identity.Name
            );

            return RedirectToAction(nameof(PendingApproval));
        }

        // DELETE
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null) return NotFound();
            return View(device);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device != null)
            {
                _context.Devices.Remove(device);
                await _context.SaveChangesAsync();

                await _audit.LogAsync(
                    id,
                    "Device Deleted",
                    device.Name,
                    null,
                    User.Identity.Name
                );
            }
            return RedirectToAction(nameof(Index));
        }

        // ADMIN OVERVIEW
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Overview()
        {
            var model = new AdminOverviewViewModel
            {
                TotalDevices = await _context.Devices.CountAsync(),
                AssignedDevices = await _context.Devices.CountAsync(d => d.TechnicianId != null),
                InProgress = await _context.Devices.CountAsync(d => d.WorkStatus == "InProgress"),
                PendingApproval = await _context.Devices.CountAsync(d => d.WorkStatus == "Done" && !d.IsApprovedByManager),
                Approved = await _context.Devices.CountAsync(d => d.IsApprovedByManager)
            };

            return View(model);
        }

        // HELPERS
        private void LoadDropdowns(int? techId = null, int? typeId = null)
        {
            ViewBag.Technicians = new SelectList(
                _context.Technicians.OrderBy(t => t.FullName),
                "Id",
                "FullName",
                techId);

            ViewBag.DeviceTypes = new SelectList(
                _context.DeviceTypes.OrderBy(t => t.Name),
                "Id",
                "Name",
                typeId);
        }
    }
}