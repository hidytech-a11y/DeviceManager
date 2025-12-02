using DeviceManager.Data;
using DeviceManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeviceManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly DeviceContext _context;

        public HomeController(DeviceContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var dashboard = new Dashboard
            {
                TotalDevices = await _context.Devices.CountAsync(),

                ActiveDevices = await _context.Devices
                    .CountAsync(d => d.Status == "Active"),

                InactiveDevices = await _context.Devices
                    .CountAsync(d => d.Status == "Inactive"),

                RecentlyAdded = await _context.Devices
                    .Include(d => d.Technician)
                    .OrderByDescending(d => d.Id)
                    .Take(5)
                    .ToListAsync(),

                AttentionNeeded = await _context.Devices
                    .Include(d => d.Technician)
                    .Where(d => d.Status == "Inactive")
                    .ToListAsync(),

                TechnicianWorkload = await _context.Technicians
                    .Select(t => new TechnicianWorkload
                    {
                        TechnicianName = t.FullName,
                        AssignedDeviceCount = t.Devices.Count
                    })
                    .ToListAsync()
            };

            return View(dashboard);
        }
    }
}
