namespace DeviceManager.Models
{
    public class Technician
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Expertise { get; set; } = string.Empty;

        public string? IdentityUserId { get; set; }

        public ICollection<Device> Devices { get; set; } = new List<Device>();


        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }

}