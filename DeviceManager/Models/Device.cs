namespace DeviceManager.Models
{
    public class Device
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }

        public int? TechnicianId { get; set; }
        public Technician? Technician { get; set; }

    }

}
