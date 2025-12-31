using DeviceManager.Data;
using DeviceManager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DeviceManager.Services
{
    public class NotificationService : INotificationService
    {
        private readonly DeviceContext _context;
        private readonly IEmailService _emailService;
        private readonly UserManager<IdentityUser> _userManager;

        public NotificationService(
            DeviceContext context,
            IEmailService emailService,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _emailService = emailService;
            _userManager = userManager;
        }

        public async Task CreateNotificationAsync(string userId, string title, string message, string type, int? deviceId = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                DeviceId = deviceId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send email
            var user = await _userManager.FindByIdAsync(userId);
            if (user?.Email != null)
            {
                //await _emailService.SendEmailAsync(user.Email, title, message);
                Console.WriteLine($"[EMAIL] To: {user.Email}, Subject: {title}"); // Debug only
            }
        }

        public async Task NotifyDeviceAssignedAsync(int deviceId, string technicianUserId)
        {
            var device = await _context.Devices
                .Include(d => d.DeviceType)
                .FirstOrDefaultAsync(d => d.Id == deviceId);

            if (device == null) return;

            var title = "New Device Assigned";
            var message = $"You have been assigned to work on device: {device.Name} ({device.SerialNumber})";
            if (device.DueDate != null)
            {
                message += $"<br/>Due Date: {device.DueDate.Value:MMMM dd, yyyy 'at' hh:mm tt}";
            }

            await CreateNotificationAsync(technicianUserId, title, message, "Info", deviceId);
        }

        public async Task NotifyDeviceDoneAsync(int deviceId)
        {
            var device = await _context.Devices
                .Include(d => d.DeviceType)
                .Include(d => d.Technician)
                .FirstOrDefaultAsync(d => d.Id == deviceId);

            if (device == null) return;

            // Notify all managers and admins
            var managers = await _userManager.GetUsersInRoleAsync("Manager");
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var allUsers = managers.Union(admins).ToList();

            var title = "Device Completed - Awaiting Approval";
            var message = $"Device {device.Name} ({device.SerialNumber}) has been marked as Done by {device.Technician?.FullName ?? "Unknown"}. Please review and approve.";

            foreach (var user in allUsers)
            {
                await CreateNotificationAsync(user.Id, title, message, "Warning", deviceId);
            }
        }

        public async Task NotifyDeviceApprovedAsync(int deviceId, string technicianUserId)
        {
            var device = await _context.Devices
                .Include(d => d.DeviceType)
                .FirstOrDefaultAsync(d => d.Id == deviceId);

            if (device == null) return;

            var title = "Device Approved";
            var message = $"Your work on device {device.Name} ({device.SerialNumber}) has been approved by the manager.";

            await CreateNotificationAsync(technicianUserId, title, message, "Success", deviceId);
        }

        public async Task NotifyDeviceOverdueAsync(int deviceId)
        {
            var device = await _context.Devices
                .Include(d => d.DeviceType)
                .Include(d => d.Technician)
                .FirstOrDefaultAsync(d => d.Id == deviceId);

            if (device == null || device.Technician == null) return;

            var technician = await _context.Technicians
                .FirstOrDefaultAsync(t => t.Id == device.TechnicianId);

            if (technician?.IdentityUserId == null) return;

            var title = "Device Overdue!";
            var message = $"Device {device.Name} ({device.SerialNumber}) is now OVERDUE. Due date was: {device.DueDate:MMMM dd, yyyy 'at' hh:mm tt}";

            await CreateNotificationAsync(technician.IdentityUserId, title, message, "Danger", deviceId);

            // Also notify managers
            var managers = await _userManager.GetUsersInRoleAsync("Manager");
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var allUsers = managers.Union(admins).ToList();

            foreach (var user in allUsers)
            {
                await CreateNotificationAsync(user.Id, title, message, "Danger", deviceId);
            }
        }

        public async Task NotifyDeviceAtRiskAsync(int deviceId)
        {
            var device = await _context.Devices
                .Include(d => d.DeviceType)
                .Include(d => d.Technician)
                .FirstOrDefaultAsync(d => d.Id == deviceId);

            if (device == null || device.Technician == null) return;

            var technician = await _context.Technicians
                .FirstOrDefaultAsync(t => t.Id == device.TechnicianId);

            if (technician?.IdentityUserId == null) return;

            var title = "Device At Risk - Due Soon";
            var message = $"Device {device.Name} ({device.SerialNumber}) is due within 24 hours! Due: {device.DueDate:MMMM dd, yyyy 'at' hh:mm tt}";

            await CreateNotificationAsync(technician.IdentityUserId, title, message, "Warning", deviceId);
        }
    }
}