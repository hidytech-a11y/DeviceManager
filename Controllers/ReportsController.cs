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

            // Make endDate inclusive of the entire day
            var endDateInclusive = endDate.Value.Date.AddDays(1).AddTicks(-1);

            var technicians = await _context.Technicians.ToListAsync();
            var performances = new List<TechnicianPerformanceViewModel>();

            foreach (var tech in technicians)
            {
                // Get completed devices in date range ONLY
                var completedDevices = await _context.Devices
                    .Where(d => d.TechnicianId == tech.Id &&
                                d.CompletedAt != null &&
                                d.CompletedAt >= startDate &&
                                d.CompletedAt <= endDateInclusive)  // Changed to inclusive
                    .ToListAsync();

                // Calculate SLA metrics
                var devicesWithDueDate = completedDevices.Where(d => d.DueDate != null).ToList();
                var metSLA = devicesWithDueDate.Count(d => d.CompletedAt <= d.DueDate);
                var missedSLA = devicesWithDueDate.Count(d => d.CompletedAt > d.DueDate);

                // Calculate average completion time
                var completionTimes = completedDevices
                    .Where(d => d.CompletedAt != null && d.DueDate != null)
                    .Select(d => Math.Abs((d.CompletedAt!.Value - d.DueDate!.Value).TotalHours))
                    .ToList();

                var avgCompletionHours = completionTimes.Any() ? completionTimes.Average() : 0;

                // Get current workload (NOT filtered by date - this should be current state)
                var currentDevices = await _context.Devices
                    .Where(d => d.TechnicianId == tech.Id && d.CompletedAt == null)
                    .ToListAsync();

                var performance = new TechnicianPerformanceViewModel
                {
                    TechnicianId = tech.Id,
                    TechnicianName = tech.FullName,
                    TotalDevicesCompleted = completedDevices.Count,
                    DevicesMetSLA = metSLA,
                    DevicesMissedSLA = missedSLA,
                    SLAComplianceRate = devicesWithDueDate.Any() ? (double)metSLA / devicesWithDueDate.Count * 100 : 0,
                    AverageCompletionHours = avgCompletionHours,
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

            // Make endDate inclusive of the entire day
            var endDateInclusive = endDate.Value.Date.AddDays(1).AddTicks(-1);

            var devices = await _context.Devices
                .Include(d => d.DeviceType)
                .Where(d => d.TechnicianId == id &&
                            d.CompletedAt != null &&
                            d.CompletedAt >= startDate &&
                            d.CompletedAt <= endDateInclusive)  // Changed to inclusive
                .OrderByDescending(d => d.CompletedAt)
                .ToListAsync();

            ViewBag.Technician = technician;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View(devices);
        }
    }
}