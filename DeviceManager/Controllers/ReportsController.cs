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

        // GET: Reports/Index
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, string sortBy = "completed")
        {
            // Default to last 30 days if no dates provided
            startDate ??= DateTime.UtcNow.AddDays(-30);
            endDate ??= DateTime.UtcNow;

            var technicians = await _context.Technicians.ToListAsync();
            var performances = new List<TechnicianPerformanceViewModel>();

            foreach (var tech in technicians)
            {
                // Get all devices for this technician
                var allDevices = await _context.Devices
                    .Where(d => d.TechnicianId == tech.Id)
                    .ToListAsync();

                // Get completed devices in date range
                var completedDevices = allDevices
                    .Where(d => d.CompletedAt != null &&
                                d.CompletedAt >= startDate &&
                                d.CompletedAt <= endDate)
                    .ToList();

                // Calculate SLA metrics
                var devicesWithDueDate = completedDevices.Where(d => d.DueDate != null).ToList();
                var metSLA = devicesWithDueDate.Count(d => d.CompletedAt <= d.DueDate);
                var missedSLA = devicesWithDueDate.Count(d => d.CompletedAt > d.DueDate);

                // Calculate average completion time
                var completionTimes = completedDevices
                    .Where(d => d.CompletedAt != null)
                    .Select(d =>
                    {
                        if (d.DueDate != null)
                        {
                            return (d.CompletedAt!.Value - d.DueDate.Value).TotalHours;
                        }
                        else
                        {
                            // If DueDate is null, use 0 or another appropriate value
                            return 0.0;
                        }
                    })
                    .ToList();

                var avgCompletionHours = completionTimes.Any() ? completionTimes.Average() : 0;

                // Get current workload
                var currentDevices = allDevices.Where(d => d.CompletedAt == null).ToList();

                var performance = new TechnicianPerformanceViewModel
                {
                    TechnicianId = tech.Id,
                    TechnicianName = tech.FullName,
                    TotalDevicesCompleted = completedDevices.Count,
                    DevicesMetSLA = metSLA,
                    DevicesMissedSLA = missedSLA,
                    SLAComplianceRate = devicesWithDueDate.Any() ? (double)metSLA / devicesWithDueDate.Count * 100 : 0,
                    AverageCompletionHours = Math.Abs(avgCompletionHours),
                    CurrentlyAssigned = currentDevices.Count,
                    InProgress = currentDevices.Count(d => d.WorkStatus == "InProgress"),
                    WaitingApproval = currentDevices.Count(d => d.WorkStatus == "Done" && !d.IsApprovedByManager),
                    CriticalDevices = currentDevices.Count(d => d.Priority == "Critical"),
                    HighDevices = currentDevices.Count(d => d.Priority == "High"),
                    MediumDevices = currentDevices.Count(d => d.Priority == "Medium"),
                    LowDevices = currentDevices.Count(d => d.Priority == "Low")
                };

                performances.Add(performance);
            }

            // Sort based on selected criteria
            performances = sortBy switch
            {
                "completed" => performances.OrderByDescending(p => p.TotalDevicesCompleted).ToList(),
                "sla" => performances.OrderByDescending(p => p.SLAComplianceRate).ToList(),
                "speed" => performances.OrderBy(p => p.AverageCompletionHours).ToList(),
                "workload" => performances.OrderByDescending(p => p.CurrentlyAssigned).ToList(),
                _ => performances.OrderByDescending(p => p.TotalDevicesCompleted).ToList()
            };

            var viewModel = new PerformanceReportViewModel
            {
                TechnicianPerformances = performances,
                StartDate = startDate,
                EndDate = endDate,
                SortBy = sortBy
            };

            return View(viewModel);
        }

        // GET: Reports/TechnicianDetail/{id}
        public async Task<IActionResult> TechnicianDetail(int id, DateTime? startDate, DateTime? endDate)
        {
            startDate ??= DateTime.UtcNow.AddDays(-30);
            endDate ??= DateTime.UtcNow;

            var technician = await _context.Technicians.FindAsync(id);
            if (technician == null) return NotFound();

            var devices = await _context.Devices
                .Include(d => d.DeviceType)
                .Where(d => d.TechnicianId == id &&
                            d.CompletedAt != null &&
                            d.CompletedAt >= startDate &&
                            d.CompletedAt <= endDate)
                .OrderByDescending(d => d.CompletedAt)
                .ToListAsync();

            ViewBag.Technician = technician;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View(devices);
        }
    }
}