namespace DeviceManager.Models
{
    public class AdminOverviewViewModel
    {
        public int TotalDevices { get; set; }
        public int AssignedDevices { get; set; }
        public int InProgress { get; set; }
        public int PendingApproval { get; set; }
        public int Approved { get; set; }
    }
}
