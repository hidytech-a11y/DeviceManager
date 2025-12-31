namespace DeviceManager.Services
{
    public interface IDeviceHistoryService
    {
        Task LogDeviceCreatedAsync(int deviceId, string userId, string userName);
        Task LogTechnicianAssignedAsync(int deviceId, string userId, string userName, string? oldTechName, string newTechName);
        Task LogStatusChangedAsync(int deviceId, string userId, string userName, string oldStatus, string newStatus);
        Task LogPriorityChangedAsync(int deviceId, string userId, string userName, string oldPriority, string newPriority);
        Task LogDueDateChangedAsync(int deviceId, string userId, string userName, DateTime? oldDate, DateTime? newDate);
        Task LogDiagnosisAddedAsync(int deviceId, string userId, string userName, string diagnosisTitle);
        Task LogDiagnosisEditedAsync(int deviceId, string userId, string userName, string diagnosisTitle);
        Task LogDiagnosisDeletedAsync(int deviceId, string userId, string userName, string diagnosisTitle);
        Task LogDeviceApprovedAsync(int deviceId, string userId, string userName);
    }
}