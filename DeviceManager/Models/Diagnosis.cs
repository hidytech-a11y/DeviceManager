namespace DeviceManager.Models
{
    public class Diagnosis
    {
        public int Id { get; set; }

        public int DeviceId { get; set; }
        public Device Device { get; set; }

        public int? TechnicianId { get; set; }
        public Technician? Technician { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Recommendation { get; set; }


        public string CreatedBy { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        //public bool IsResolved { get; set; }
    }
}
