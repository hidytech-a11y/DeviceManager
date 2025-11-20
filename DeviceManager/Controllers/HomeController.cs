using DeviceManager.Data;
using DeviceManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class HomeController : Controller
{
    private readonly DeviceContext _context;

    public HomeController(DeviceContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var vm = new Dashboard
        {
            TotalDevices = _context.Devices.Count(),
            ActiveDevices = _context.Devices.Count(d => d.Status == "Active"),
            InactiveDevices = _context.Devices.Count(d => d.Status == "Inactive"),

            RecentlyAdded = _context.Devices
                .OrderByDescending(d => d.Id)
                .Take(5)
                .ToList(),

            AttentionNeeded = _context.Devices
                .Where(d => d.Status == "Inactive")
                .ToList()
        };

        return View(vm);
    }
}
