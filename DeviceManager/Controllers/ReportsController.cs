using System.Linq;
using System.Threading.Tasks;
using DeviceManager.Data;
using DeviceManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace DeviceManager.Controllers
{
    [Authorize(Roles = "Admin,Manager,Viewer")]
    public class ReportsController : Controller
    {
        private readonly DeviceContext _context;

        public ReportsController(DeviceContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var devices = _context.Devices.AsQueryable();

            var model = new ReportsViewModel
            {
                TotalDevices = await devices.CountAsync(),
                ActiveDevices = await devices.CountAsync(d => d.Status == "Active"),
                InactiveDevices = await devices.CountAsync(d => d.Status == "Inactive"),

                AssignedDevices = await devices.CountAsync(d => d.TechnicianId != null),
                UnassignedDevices = await devices.CountAsync(d => d.TechnicianId == null),

                AssignedTasks = await devices.CountAsync(d => d.WorkStatus == "Assigned"),
                InProgressTasks = await devices.CountAsync(d => d.WorkStatus == "InProgress"),
                CompletedTasks = await devices.CountAsync(d => d.WorkStatus == "Done"),

                PendingApprovals = await devices.CountAsync(d => d.WorkStatus == "Done" && !d.IsApprovedByManager),
                ApprovedTasks = await devices.CountAsync(d => d.IsApprovedByManager)
            };

            model.TechnicianStats = await _context.Technicians
                .Select(t => new TechnicianReportItem
                {
                    TechnicianName = t.FullName,
                    TotalAssigned = t.Devices.Count(),
                    InProgress = t.Devices.Count(d => d.WorkStatus == "InProgress"),
                    Completed = t.Devices.Count(d => d.WorkStatus == "Done")
                })
                .ToListAsync();

            return View(model);
        }
    }

}

