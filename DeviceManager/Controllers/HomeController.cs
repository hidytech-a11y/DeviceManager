using DeviceManager.Data;
using DeviceManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeviceManager.Controllers
{
    public class HomeController(DeviceContext context) : Controller
    {
        private readonly DeviceContext _context = context;

        public async Task<IActionResult> Index()
        {
            var devicesQuery = _context.Devices
                .Include(d => d.Technician)
                .Include(d => d.DeviceType)
                .AsQueryable();

            var total = await devicesQuery.CountAsync();
            var active = await devicesQuery.CountAsync(d => d.Status == "Active");
            var inactive = await devicesQuery.CountAsync(d => d.Status == "Inactive");
            var assigned = await devicesQuery.CountAsync(d => d.TechnicianId != null);
            var unassigned = await devicesQuery.CountAsync(d => d.TechnicianId == null);

            var devicesByType = await devicesQuery
                .GroupBy(d => d.DeviceType != null ? d.DeviceType.Name : "Unknown")
                .Select(g => new ChartGroup
                {
                    Label = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var recentlyAdded = await devicesQuery
                .OrderByDescending(d => d.Id)
                .Take(5)
                .ToListAsync();

            var attentionNeeded = await devicesQuery
                .Where(d => d.Status == "Inactive")
                .ToListAsync();

            var technicianWorkload = await _context.Technicians
                .Select(t => new TechnicianWorkload
                {
                    TechnicianName = t.FullName,
                    AssignedDeviceCount = t.Devices.Count
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
                RecentlyAdded = recentlyAdded,
                AttentionNeeded = attentionNeeded,
                TechnicianWorkload = technicianWorkload
            };

            return View(dashboard);
        }
    }
}