using DeviceManager.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System.IO;

namespace DeviceManager.Controllers
{
    [Authorize(Roles = "Admin,Manager,Viewer")]
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

            ViewBag.DeviceId = deviceId;
            return View(logs);
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ExportDeviceAudit(int deviceId)
        {
            var logs = await _context.AuditLogs
                .Where(a => a.DeviceId == deviceId)
                .OrderByDescending(a => a.PerformedAt)
                .AsNoTracking()
                .ToListAsync();

            if (!logs.Any())
                return BadRequest("No audit logs found.");

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Audit Trail");

            sheet.Cell(1, 1).Value = "Date";
            sheet.Cell(1, 2).Value = "Action";
            sheet.Cell(1, 3).Value = "Old Value";
            sheet.Cell(1, 4).Value = "New Value";
            sheet.Cell(1, 5).Value = "User";

            int row = 2;

            foreach (var log in logs)
            {
                sheet.Cell(row, 1).Value = log.PerformedAt;
                sheet.Cell(row, 2).Value = log.Action;
                sheet.Cell(row, 3).Value = log.OldValue ?? "";
                sheet.Cell(row, 4).Value = log.NewValue ?? "";
                sheet.Cell(row, 5).Value = log.PerformedBy ?? "";
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Device_{deviceId}_AuditTrail.xlsx"
            );
        }
    }
}
