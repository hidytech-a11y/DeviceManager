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
            var total = await _context.Devices.CountAsync();

            var active = await _context.Devices
                .CountAsync(d => d.Status == "Active");

            var inactive = await _context.Devices
                .CountAsync(d => d.Status == "Inactive");

            var assigned = await _context.Devices
                .CountAsync(d => d.TechnicianId != null);

            var unassigned = await _context.Devices
                .CountAsync(d => d.TechnicianId == null);

            var devicesByType = await _context.Devices
                .GroupBy(d => d.Type)
                .Select(g => new ChartGroup
                {
                    Label = g.Key ?? "Unknown",
                    Count = g.Count()
                })
                .ToListAsync();


            var dashboard = new Dashboard
            {
                TotalDevices = total,
                ActiveDevices = active,
                InactiveDevices = inactive,
                AssignedDevices = assigned,
                UnassignedDevices = unassigned,
                DevicesByType = devicesByType,

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
