using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace DeviceManager.Models
{
    public class Dashboard
    {
        public int TotalDevices { get; set; }
        public int ActiveDevices { get; set; }
        public int InactiveDevices { get; set; }

        public int AssignedDevices { get; set; }
        public int UnassignedDevices { get; set; }

        public List<Device> RecentlyAdded { get; set; } = [];
        public List<Device> AttentionNeeded { get; set; } = [];

        public List<TechnicianWorkload> TechnicianWorkload { get; set; } = [];
        public List<ChartGroup> DevicesByType { get; set; } = [];
    }

    public class TechnicianWorkload
    {
        public string TechnicianName { get; set; } = string.Empty;
        public int AssignedDeviceCount { get; set; }
    }

    public class ChartGroup
    {
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
