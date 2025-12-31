namespace DeviceManager.Models
{
    public class DeviceListViewModel
    {
        public IEnumerable<Device> Devices { get; set; } = [];
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public string Search { get; set; } = "";
        public string SortOrder { get; set; } = "";
        public string TypeFilter { get; set; } = "";
        public string StatusFilter { get; set; } = "";
        public string PriorityFilter { get; set; } = "";
        public string SLAFilter { get; set; } = "";  // Add this line
        public int? TechnicianId { get; set; }
        public int TotalDevices { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
    }
}