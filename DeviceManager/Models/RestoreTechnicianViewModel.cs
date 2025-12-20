using System.Collections.Generic;

namespace DeviceManager.Models
{
    public class RestoreTechnicianViewModel
    {
        public int TechnicianId { get; set; }   
        public string FullName { get; set; } = string.Empty;



        public List<int> SelectedDeviceIds { get; set; } = new();
        public List<Device> AvailableDevices { get; set; } = new();
    }
}
