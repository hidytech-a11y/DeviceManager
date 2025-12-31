namespace DeviceManager.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(string userId, string title, string message, string type, int? deviceId = null);
        Task NotifyDeviceAssignedAsync(int deviceId, string technicianUserId);
        Task NotifyDeviceDoneAsync(int deviceId);
        Task NotifyDeviceApprovedAsync(int deviceId, string technicianUserId);
        Task NotifyDeviceOverdueAsync(int deviceId);
        Task NotifyDeviceAtRiskAsync(int deviceId);
    }
}