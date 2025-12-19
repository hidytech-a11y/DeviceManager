namespace DeviceManager.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        public int Device { get; set; }

        public int DeviceId { get; set; }

        public string Action { get; set; } = string.Empty;

        public string? OldValue { get; set; }   // MUST be nullable
        public string? NewValue { get; set; }   // MUST be nullable

        public string PerformedBy { get; set; } = string.Empty;

        public DateTime PerformedAt { get; set; }
    }

}
