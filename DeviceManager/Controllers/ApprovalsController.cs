using DeviceManager.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeviceManager.Controllers
{
    [Authorize]
    public class ApprovalsController : Controller
    {

        private readonly DeviceContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ApprovalsController(
     DeviceContext context,
     UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
            var userId = _userManager.GetUserId(User);

            var device = await _context.Devices.FindAsync(id);
            if (device == null)
                return NotFound();

            device.IsApprovedByManager = true;
            device.ApprovedByManagerId = userId;
            device.ApprovedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }


}
