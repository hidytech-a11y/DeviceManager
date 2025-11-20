namespace DeviceManager.Models
{
    public class Technician
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Expertise { get; set; }

        // REQUIRED — EF expects this because your controller uses it
        public List<Device> Devices { get; set; } = new();
    }
}
