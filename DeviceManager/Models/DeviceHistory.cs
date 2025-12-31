namespace DeviceManager.Models
{
    public class DeviceHistory
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public Device? Device { get; set; }
        public string Action { get; set; } = string.Empty; // "Created", "Assigned", "StatusChanged", etc.
        public string Description { get; set; } = string.Empty;
        public string? PerformedByUserId { get; set; }
        public string? PerformedByName { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
    }
}