namespace DeviceManager.Models
{
    public class TechnicianPerformanceViewModel
    {
        public string TechnicianName { get; set; } = string.Empty;
        public int TotalAssigned { get; set; }
        public int InProgress { get; set; }
        public int Completed { get; set; }
        public int Approved { get; set; }
        public int PendingApproval { get; set; }
    }

}

