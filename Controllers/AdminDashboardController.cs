using DeviceManager.Data;
using DeviceManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeviceManager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly UserManager<IdentityUser> _users;
        private readonly RoleManager<IdentityRole> _roles;
        private readonly DeviceContext _db;
        private readonly IConfiguration _config;

        public AdminDashboardController(
            UserManager<IdentityUser> users,
            RoleManager<IdentityRole> roles,
            DeviceContext db,
            IConfiguration config)
        {
            _users = users;
            _roles = roles;
            _db = db;
            _config = config;
        }

        public async Task<IActionResult> Index()
        {
            var totalUsers = await _users.Users.CountAsync();
            var totalDevices = await _db.Devices.CountAsync();
            var totalTechnicians = await _users.GetUsersInRoleAsync("Technician");
            var totalManagers = await _users.GetUsersInRoleAsync("Manager");

            var model = new AdminDashboardViewModel
            {
                UserCount = totalUsers,
                DeviceCount = totalDevices,
                TechnicianCount = totalTechnicians.Count,
                ManagerCount = totalManagers.Count
            };

            ViewBag.AdminOverrideEnabled =
                _config.GetValue<bool>("AdminOverride:Enabled");

            return View(model);
        }

        public IActionResult Users()
        {
            var all = _users.Users.ToList();
            return View(all);
        }

        public IActionResult Roles()
        {
            var all = _roles.Roles.ToList();
            return View(all);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleOverride()
        {
            var current = _config.GetValue<bool>("AdminOverride:Enabled");

            if (!current)
            {
                _config["AdminOverride:Enabled"] = "true";
                _config["AdminOverride:ExpiresAt"] = DateTime.UtcNow.AddMinutes(30).ToString("O");
            }
            else
            {
                _config["AdminOverride:Enabled"] = "false";
            }


            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}
