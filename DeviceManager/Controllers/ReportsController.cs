using System.Linq;
using System.Threading.Tasks;
using DeviceManager.Data;
using DeviceManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;


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

                PendingApprovals = await devices.CountAsync(
                    d => d.WorkStatus == "Done" && !d.IsApprovedByManager
                ),
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

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ExportExcel()
        {
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("System Report");

            sheet.Cell(1, 1).Value = "Metric";
            sheet.Cell(1, 2).Value = "Value";

            sheet.Cell(2, 1).Value = "Total Devices";
            sheet.Cell(2, 2).Value = await _context.Devices.CountAsync();

            sheet.Cell(3, 1).Value = "Active Devices";
            sheet.Cell(3, 2).Value = await _context.Devices.CountAsync(d => d.Status == "Active");

            sheet.Cell(4, 1).Value = "Inactive Devices";
            sheet.Cell(4, 2).Value = await _context.Devices.CountAsync(d => d.Status == "Inactive");

            sheet.Cell(5, 1).Value = "Assigned Tasks";
            sheet.Cell(5, 2).Value = await _context.Devices.CountAsync(d => d.WorkStatus == "Assigned");

            sheet.Cell(6, 1).Value = "In Progress Tasks";
            sheet.Cell(6, 2).Value = await _context.Devices.CountAsync(d => d.WorkStatus == "InProgress");

            sheet.Cell(7, 1).Value = "Completed Tasks";
            sheet.Cell(7, 2).Value = await _context.Devices.CountAsync(d => d.WorkStatus == "Done");

            sheet.Cell(8, 1).Value = "Pending Approvals";
            sheet.Cell(8, 2).Value = await _context.Devices.CountAsync(
                d => d.WorkStatus == "Done" && !d.IsApprovedByManager
            );

            sheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Device_Report.xlsx"
            );
        }

        [Authorize(Roles = " Admin,Manager,Viewer")]
        public async Task<IActionResult> TechnicianPerformance()
        {
            var data = await _context.Technicians
                .Select(t => new TechnicianPerformanceViewModel
                {
                    TechnicianName = t.FullName,
                    TotalAssigned = t.Devices.Count(),
                    InProgress = t.Devices.Count(d => d.WorkStatus == "InProgress"),
                    Completed = t.Devices.Count(d => d.WorkStatus == "Done"),
                    Approved = t.Devices.Count(d => d.IsApprovedByManager),
                    PendingApproval = t.Devices.Count(
                        d => d.WorkStatus == "Done" && !d.IsApprovedByManager
                    )
                })
                .OrderBy(t => t.TechnicianName)
                .ToListAsync();
            return View(data);
        }
    }


}
 


