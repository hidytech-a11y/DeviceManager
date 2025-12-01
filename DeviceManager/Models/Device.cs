using System.ComponentModel.DataAnnotations;

namespace DeviceManager.Models
{
    public class Device
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = string.Empty;

        [Required]
        public string SerialNumber { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Active";

        public int? TechnicianId { get; set; }
        public Technician? Technician { get; set; }

        public int? DeviceTypeId { get; set; }
        public DeviceType? DeviceType { get; set; }
    }
}
