using System.ComponentModel.DataAnnotations;
namespace DeviceManager.Models
{
    public class Device
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string SerialNumber { get; set; } = string.Empty;
        [Required]
        public string Status { get; set; } = "Active";
        public int? TechnicianId { get; set; }
        public Technician? Technician { get; set; }
        public int? DeviceTypeId { get; set; }
        public DeviceType? DeviceType { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedByManagerId { get; set; }
        public bool IsApprovedByManager { get; set; }
        public string WorkStatus { get; set; } = "Assigned";
        public string Priority { get; set; } = "Medium";
        public DateTime? DueDate { get; set; }
        public string? SLAStatus { get; set; }
        public ICollection<Diagnosis> Diagnoses { get; set; } = new List<Diagnosis>();

        // Helper method to calculate SLA status
        public string GetSLAStatus()
        {
            if (DueDate == null) return "No Due Date";

            var now = DateTime.UtcNow;
            var hoursUntilDue = (DueDate.Value - now).TotalHours;

            // If completed
            if (CompletedAt != null)
            {
                return CompletedAt <= DueDate ? "Met SLA" : "Missed SLA";
            }

            // If not completed yet
            if (now > DueDate.Value)
            {
                return "Overdue";
            }
            else if (hoursUntilDue <= 24)
            {
                return "At Risk";
            }
            else
            {
                return "On Time";
            }
        }
    }
}