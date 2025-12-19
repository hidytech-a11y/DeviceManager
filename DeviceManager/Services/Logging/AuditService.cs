using DeviceManager.Data;

namespace DeviceManager.Services.Logging
{
    public class AuditService : IAuditService
    {
        private readonly DeviceContext _context;
        public AuditService(DeviceContext context)
        {
            _context = context;
        }
    

    public async Task LogAsync(
        int deviceId,
        string action,
        string oldValue,
        string newValue,
        string userName
        )
        {
            _context.AuditLogs.Add(new Models.AuditLog
            {
                DeviceId = deviceId,
                Action = action,
                OldValue = oldValue,
                NewValue = newValue,
                PerformedBy = userName,
                PerformedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

        }
    }
}
