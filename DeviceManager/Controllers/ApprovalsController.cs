using DeviceManager.Data;
using DeviceManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeviceManager.Controllers
{
    [Authorize(Policy = "ManagerOrAdminOverride")]
    public class ApprovalsController : Controller
    {

        private readonly DeviceContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAdminOverrideService _override;
        private readonly INotificationService _notificationService;

        public ApprovalsController(
             DeviceContext context,
             UserManager<IdentityUser> userManager,
             IAdminOverrideService overrideService,
             INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _override = overrideService;
            _notificationService = notificationService;
        }

        // GET: /Approvals
        public async Task<IActionResult> Index()
        {
            var devices = await _context.Devices
                .Include(d => d.Technician)
                .Include(d => d.DeviceType)
                .Where(d => d.WorkStatus == "Done" && !d.IsApprovedByManager)
                .ToListAsync();

            return View(devices);
        }

        // POST: /Approvals/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var isAdminOverride =
                User.IsInRole("Admin") && _override.IsEnabled();
            if (!isAdminOverride && !User.IsInRole("Manager"))
                return Unauthorized();

            var device = await _context.Devices
                .Include(d => d.Technician)  // Add this to load technician
                .FirstOrDefaultAsync(d => d.Id == id);

            if (device == null)
                return NotFound();

            device.IsApprovedByManager = true;
            device.ApprovedByManagerId = _userManager.GetUserId(User);
            device.ApprovedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Add notification trigger
            if (device.Technician?.IdentityUserId != null)
            {
                await _notificationService.NotifyDeviceApprovedAsync(id, device.Technician.IdentityUserId);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
