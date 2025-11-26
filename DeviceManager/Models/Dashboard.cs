using Microsoft.AspNetCore.Mvc;

namespace DeviceManager.Models
{
    public class Dashboard : Controller
    {
        public int TotalDevices { get; set; }
        public int ActiveDevices { get; set; }
        public int InactiveDevices { get; set; }

        public List<Device> RecentlyAdded { get; set; } = new();
        public List<Device> AttentionNeeded { get; set; } = new();

        public List<TechnicianWorkload> TechnicianWorkload { get; set; } = new();
    }

    public class TechnicianWorkload
    {
        public string TechnicianName { get; set; } = string.Empty;
        public int AssignedDeviceCount { get; set; }
    }
}
