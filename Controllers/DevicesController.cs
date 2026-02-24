using DeviceManager.Data;
using DeviceManager.Models;
using DeviceManager.Services;
using DeviceManager.Services.Logging;
using DocumentFormat.OpenXml.Office2021.DocumentTasks;
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
        private readonly IAdminOverrideService _override;
        private readonly INotificationService _notificationService;
        private readonly IDeviceHistoryService _historyService;
        private const int PageSize = 10;

        public DevicesController(
            DeviceContext context,
            ILogger<DevicesController> logger,
            UserManager<IdentityUser> userManager,
            IAuditService audit,
            INotificationService notificationService,
            IAdminOverrideService overrideService,
            IDeviceHistoryService historyService)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _audit = audit;
            _override = overrideService;
            _notificationService = notificationService;
            _historyService = historyService;
        }

        // LIST
        [Authorize(Roles = "Admin,Technician,Viewer,Manager")]
        public async Task<IActionResult> Index(
                 string search,
                 string sortOrder,
                 string typeFilter,
                 string statusFilter,
                 string priorityFilter,
                 string slaFilter,  // Add this parameter
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

            if (!string.IsNullOrWhiteSpace(priorityFilter))
                query = query.Where(d => d.Priority == priorityFilter);

            if (technicianId.HasValue)
                query = query.Where(d => d.TechnicianId == technicianId);

            // Get all devices first for SLA filtering (needs to be done in memory)
            var allDevices = await query.ToListAsync();

            // Apply SLA filter
            if (!string.IsNullOrWhiteSpace(slaFilter))
            {
                allDevices = slaFilter switch
                {
                    "Overdue" => allDevices.Where(d => d.GetSLAStatus() == "Overdue").ToList(),
                    "AtRisk" => allDevices.Where(d => d.GetSLAStatus() == "At Risk").ToList(),
                    "OnTime" => allDevices.Where(d => d.GetSLAStatus() == "On Time").ToList(),
                    _ => allDevices
                };
            }

            // Apply sorting
            allDevices = sortOrder switch
            {
                "name_desc" => allDevices.OrderByDescending(d => d.Name).ToList(),
                "type" => allDevices.OrderBy(d => d.DeviceType?.Name).ToList(),
                "type_desc" => allDevices.OrderByDescending(d => d.DeviceType?.Name).ToList(),
                "priority" => allDevices.OrderBy(d => d.Priority == "Critical" ? 1 : d.Priority == "High" ? 2 : d.Priority == "Medium" ? 3 : 4).ToList(),
                "priority_desc" => allDevices.OrderByDescending(d => d.Priority == "Critical" ? 1 : d.Priority == "High" ? 2 : d.Priority == "Medium" ? 3 : 4).ToList(),
                "duedate" => allDevices.OrderBy(d => d.DueDate ?? DateTime.MaxValue).ToList(),
                "duedate_desc" => allDevices.OrderByDescending(d => d.DueDate ?? DateTime.MinValue).ToList(),
                _ => allDevices.OrderBy(d => d.Name).ToList()
            };

            var totalItems = allDevices.Count;
            var devices = allDevices
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            var vm = new DeviceListViewModel
            {
                Devices = devices,
                PageNumber = pageNumber,
                TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize),
                Search = search,
                SortOrder = sortOrder,
                TypeFilter = typeFilter,
                StatusFilter = statusFilter,
                PriorityFilter = priorityFilter,
                SLAFilter = slaFilter,  // Add this line
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

            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;
            var userName = user.Email ?? "Unknown";

            // Log history
            await _historyService.LogDeviceCreatedAsync(device.Id, userId, userName);

            await _audit.LogAsync(
                device.Id,
                "Device Created",
                null,
                device.Name,
                User.Identity.Name
            );

            if (device.TechnicianId != null)
            {
                var technician = await _context.Technicians
                    .FirstOrDefaultAsync(t => t.Id == device.TechnicianId);

                if (technician?.IdentityUserId != null)
                {
                    // Log technician assignment
                    await _historyService.LogTechnicianAssignedAsync(
                        device.Id, userId, userName, null, technician.FullName);

                    await _notificationService.NotifyDeviceAssignedAsync(device.Id, technician.IdentityUserId);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // EDIT
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var device = await _context.Devices
                .Include(d => d.Technician)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (device == null) return NotFound();

            LoadDropdowns(device.TechnicianId, device.DeviceTypeId);
            return View(device);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Device device)
        {
            var existing = await _context.Devices
                .Include(d => d.Technician)
                .FirstOrDefaultAsync(d => d.Id == device.Id);

            if (existing == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;
            var userName = user.Email ?? "Unknown";

            var oldStatus = existing.Status;
            var oldTechId = existing.TechnicianId;
            var oldTechName = existing.Technician?.FullName;
            var oldPriority = existing.Priority;
            var oldDueDate = existing.DueDate;

            existing.Name = device.Name;
            existing.SerialNumber = device.SerialNumber;
            existing.Status = device.Status;
            existing.Priority = device.Priority;
            existing.DueDate = device.DueDate;
            existing.DeviceTypeId = device.DeviceTypeId == 0 ? null : device.DeviceTypeId;

            // Track technician change
            if (oldTechId != device.TechnicianId)
            {
                existing.TechnicianId = device.TechnicianId == 0 ? null : device.TechnicianId;
                existing.WorkStatus = existing.TechnicianId == null ? null : "Assigned";
                existing.IsApprovedByManager = false;

                var newTechnician = existing.TechnicianId != null
                    ? await _context.Technicians.FindAsync(existing.TechnicianId)
                    : null;

                await _historyService.LogTechnicianAssignedAsync(
                    existing.Id, userId, userName, oldTechName, newTechnician?.FullName ?? "Unassigned");

                await _audit.LogAsync(
                    existing.Id,
                    "Technician Changed",
                    oldTechId?.ToString(),
                    existing.TechnicianId?.ToString(),
                    User.Identity.Name
                );

                if (newTechnician?.IdentityUserId != null)
                {
                    await _notificationService.NotifyDeviceAssignedAsync(device.Id, newTechnician.IdentityUserId);
                }
            }

            // Track status change
            if (oldStatus != existing.Status)
            {
                await _historyService.LogStatusChangedAsync(
                    existing.Id, userId, userName, oldStatus, existing.Status);

                await _audit.LogAsync(
                    existing.Id,
                    "Status Changed",
                    oldStatus,
                    existing.Status,
                    User.Identity.Name
                );
            }

            // Track priority change
            if (oldPriority != existing.Priority)
            {
                await _historyService.LogPriorityChangedAsync(
                    existing.Id, userId, userName, oldPriority, existing.Priority);
            }

            // Track due date change
            if (oldDueDate != existing.DueDate)
            {
                await _historyService.LogDueDateChangedAsync(
                    existing.Id, userId, userName, oldDueDate, existing.DueDate);
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
                .Include(d => d.Diagnoses)
                    .ThenInclude(diag => diag.Technician)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (device == null) return NotFound();

            // Load history for timeline
            ViewBag.History = await _context.DeviceHistories
                .Where(h => h.DeviceId == id)
                .OrderByDescending(h => h.Timestamp)
                .ToListAsync();

            return View(device);
        }

        // TECHNICIAN STATUS UPDATE
        [Authorize(Roles = "Admin,Technician")]
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            if (!User.IsInRole("Technician") &&
                !(User.IsInRole("Admin") && _override.IsEnabled()))
                return Forbid();

            var device = await _context.Devices.FindAsync(id);
            if (device == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;
            var userName = user.Email ?? "Unknown";

            var old = device.WorkStatus;
            device.WorkStatus = status;

            if (status == "Done")
            {
                device.CompletedAt = DateTime.UtcNow;
                await _notificationService.NotifyDeviceDoneAsync(id);
            }

            await _context.SaveChangesAsync();

            // Log history
            await _historyService.LogStatusChangedAsync(device.Id, userId, userName, old, status);

            await _audit.LogAsync(
                device.Id,
                "Work Status Updated (Override)",
                old,
                status,
                User.Identity.Name
            );

            return RedirectToAction("Index");
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
        [Authorize(Roles = "Admin,Manager")]
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            if (!User.IsInRole("Manager") &&
                !(User.IsInRole("Admin") && _override.IsEnabled()))
                return Forbid();

            var device = await _context.Devices
                .Include(d => d.Technician)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (device == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;
            var userName = user.Email ?? "Unknown";

            device.IsApprovedByManager = true;
            device.ApprovedByManagerId = User.Identity.Name;
            device.ApprovedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Log history
            await _historyService.LogDeviceApprovedAsync(device.Id, userId, userName);

            await _audit.LogAsync(
                device.Id,
                "Manager Approval (Override)",
                "Pending",
                "Approved",
                User.Identity.Name
            );

            if (device.Technician?.IdentityUserId != null)
            {
                await _notificationService.NotifyDeviceApprovedAsync(id, device.Technician.IdentityUserId);
            }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDiagnosis(int DeviceId, string Title, string Description, string Recommendation)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);
            var userName = user.Email ?? "Unknown";

            var technician = await _context.Technicians
                .FirstOrDefaultAsync(t => t.IdentityUserId == userId);

            var diagnosis = new Diagnosis
            {
                DeviceId = DeviceId,
                Title = Title,
                Description = Description,
                Recommendation = Recommendation,
                CreatedAt = DateTime.UtcNow,
                TechnicianId = technician?.Id
            };

            _context.Diagnoses.Add(diagnosis);
            await _context.SaveChangesAsync();

            // Log history
            await _historyService.LogDiagnosisAddedAsync(DeviceId, userId, userName, Title);

            TempData["SuccessMessage"] = "Diagnosis saved successfully!";
            TempData["ExpandDeviceId"] = DeviceId.ToString();

            return RedirectToAction("MyTasks", "Technicians");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDiagnosis(int id, string title, string description, string recommendation)
        {
            var diagnosis = await _context.Diagnoses.FindAsync(id);
            if (diagnosis == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);
            var userName = user.Email ?? "Unknown";

            diagnosis.Title = title;
            diagnosis.Description = description;
            diagnosis.Recommendation = recommendation;

            await _context.SaveChangesAsync();

            // Log history
            await _historyService.LogDiagnosisEditedAsync(diagnosis.DeviceId, userId, userName, title);

            TempData["SuccessMessage"] = "Diagnosis updated successfully!";
            TempData["ExpandDeviceId"] = diagnosis.DeviceId.ToString();

            return RedirectToAction("MyTasks", "Technicians");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDiagnosis(int id)
        {
            var diagnosis = await _context.Diagnoses.FindAsync(id);
            if (diagnosis == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);
            var userName = user.Email ?? "Unknown";
            var deviceId = diagnosis.DeviceId;
            var title = diagnosis.Title;

            _context.Diagnoses.Remove(diagnosis);
            await _context.SaveChangesAsync();

            // Log history
            await _historyService.LogDiagnosisDeletedAsync(deviceId, userId, userName, title);

            TempData["SuccessMessage"] = "Diagnosis deleted successfully!";
            TempData["ExpandDeviceId"] = deviceId.ToString();

            return RedirectToAction("MyTasks", "Technicians");
        }


    }
}