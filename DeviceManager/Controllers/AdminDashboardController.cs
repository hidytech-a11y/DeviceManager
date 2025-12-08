using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using DeviceManager.Models;
using Microsoft.EntityFrameworkCore;
using DeviceManager.Data;

namespace DeviceManager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController(
        UserManager<IdentityUser> users,
        RoleManager<IdentityRole> roles,
        DeviceContext db) : Controller
    {
        private readonly UserManager<IdentityUser> _users = users;
        private readonly RoleManager<IdentityRole> _roles = roles;
        private readonly DeviceContext _db = db;

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

            return View(model);
        }

        public async Task<IActionResult> Users()
        {
            var all = _users.Users.ToList();
            return View(all);
        }

        public async Task<IActionResult> Roles()
        {
            var all = _roles.Roles.ToList();
            return View(all);
        }
    }
}
