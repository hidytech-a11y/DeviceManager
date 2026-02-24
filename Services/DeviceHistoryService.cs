using DeviceManager.Data;
using DeviceManager.Models;

namespace DeviceManager.Services
{
    public class DeviceHistoryService : IDeviceHistoryService
    {
        private readonly DeviceContext _context;

        public DeviceHistoryService(DeviceContext context)
        {
            _context = context;
        }

        private async Task LogHistoryAsync(int deviceId, string action, string description, string userId, string userName, string? oldValue = null, string? newValue = null)
        {
            var history = new DeviceHistory
            {
                DeviceId = deviceId,
                Action = action,
                Description = description,
                PerformedByUserId = userId,
                PerformedByName = userName,
                Timestamp = DateTime.UtcNow,
                OldValue = oldValue,
                NewValue = newValue
            };

            _context.DeviceHistories.Add(history);
            await _context.SaveChangesAsync();
        }

        public async Task LogDeviceCreatedAsync(int deviceId, string userId, string userName)
        {
            await LogHistoryAsync(deviceId, "Created", $"Device created by {userName}", userId, userName);
        }

        public async Task LogTechnicianAssignedAsync(int deviceId, string userId, string userName, string? oldTechName, string newTechName)
        {
            var description = oldTechName == null
                ? $"Device assigned to {newTechName}"
                : $"Device reassigned from {oldTechName} to {newTechName}";

            await LogHistoryAsync(deviceId, "Assigned", description, userId, userName, oldTechName, newTechName);
        }

        public async Task LogStatusChangedAsync(int deviceId, string userId, string userName, string oldStatus, string newStatus)
        {
            await LogHistoryAsync(deviceId, "StatusChanged", $"Work status changed from {oldStatus} to {newStatus}", userId, userName, oldStatus, newStatus);
        }

        public async Task LogPriorityChangedAsync(int deviceId, string userId, string userName, string oldPriority, string newPriority)
        {
            await LogHistoryAsync(deviceId, "PriorityChanged", $"Priority changed from {oldPriority} to {newPriority}", userId, userName, oldPriority, newPriority);
        }

        public async Task LogDueDateChangedAsync(int deviceId, string userId, string userName, DateTime? oldDate, DateTime? newDate)
        {
            var oldDateStr = oldDate?.ToString("MMM dd, yyyy") ?? "None";
            var newDateStr = newDate?.ToString("MMM dd, yyyy") ?? "None";

            await LogHistoryAsync(deviceId, "DueDateChanged", $"Due date changed from {oldDateStr} to {newDateStr}", userId, userName, oldDateStr, newDateStr);
        }

        public async Task LogDiagnosisAddedAsync(int deviceId, string userId, string userName, string diagnosisTitle)
        {
            await LogHistoryAsync(deviceId, "DiagnosisAdded", $"Diagnosis added: {diagnosisTitle}", userId, userName);
        }

        public async Task LogDiagnosisEditedAsync(int deviceId, string userId, string userName, string diagnosisTitle)
        {
            await LogHistoryAsync(deviceId, "DiagnosisEdited", $"Diagnosis edited: {diagnosisTitle}", userId, userName);
        }

        public async Task LogDiagnosisDeletedAsync(int deviceId, string userId, string userName, string diagnosisTitle)
        {
            await LogHistoryAsync(deviceId, "DiagnosisDeleted", $"Diagnosis deleted: {diagnosisTitle}", userId, userName);
        }

        public async Task LogDeviceApprovedAsync(int deviceId, string userId, string userName)
        {
            await LogHistoryAsync(deviceId, "Approved", $"Device work approved by {userName}", userId, userName);
        }
    }
}