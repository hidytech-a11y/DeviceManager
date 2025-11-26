namespace DeviceManager.Models
{
    public class Technician
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Expertise { get; set; } = string.Empty;


        public ICollection<Device> Devices { get; set; } = new List<Device>();
    }
}
