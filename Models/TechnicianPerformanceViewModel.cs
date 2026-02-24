namespace DeviceManager.Models
{
    public class TechnicianPerformanceViewModel
    {
        public int TechnicianId { get; set; }
        public string TechnicianName { get; set; } = string.Empty;
        public int TotalDevicesCompleted { get; set; }
        public int DevicesMetSLA { get; set; }
        public int DevicesMissedSLA { get; set; }
        public double SLAComplianceRate { get; set; }
        public double AverageCompletionHours { get; set; }
        public int CurrentlyAssigned { get; set; }
        public int InProgress { get; set; }
        public int WaitingApproval { get; set; }
        public int CriticalDevices { get; set; }
        public int HighDevices { get; set; }
        public int MediumDevices { get; set; }
        public int LowDevices { get; set; }
    }

    public class PerformanceReportViewModel
    {
        public List<TechnicianPerformanceViewModel> TechnicianPerformances { get; set; } = new();
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SortBy { get; set; } = "completed";
    }
}