namespace DeviceManager.Services.Logging
{
    public interface IAuditService
    {
        Task LogAsync(
            int deviceId,
            string action,
            string oldValue,
            string newValue,
            string userName
            );
    }
}
