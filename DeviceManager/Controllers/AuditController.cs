using DeviceManager.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeviceManager.Controllers
{

    [Authorize(Roles = "Admin,Mnagaer,Viewer")]
    public class AuditController : Controller
    {
        private readonly DeviceContext _context;

        public AuditController(DeviceContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Device(int deviceId)
        {
            var logs = await _context.AuditLogs
                .Where(a => a.DeviceId == deviceId)
                .OrderByDescending(a => a.PerformedAt)
                .AsNoTracking()
                .ToListAsync();

            return View(logs);
        }

    }
}
