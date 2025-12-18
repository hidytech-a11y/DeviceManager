using System.Collections.Generic;

namespace DeviceManager.Models
{
    public class ReportsViewModel
    {
        public int TotalDevices { get; set; }
        public int ActiveDevices { get; set; }
        public int InactiveDevices { get; set; }

        public int AssignedDevices { get; set; }
        public int UnassignedDevices { get; set; }

        public int AssignedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CompletedTasks { get; set; }

        public int PendingApprovals { get; set; }
        public int ApprovedTasks { get; set; }

        public List<TechnicianReportItem> TechnicianStats { get; set; } = new();
    }

    public class TechnicianReportItem
    {
        public string TechnicianName { get; set; }
        public int TotalAssigned { get; set; }
        public int InProgress { get; set; }
        public int Completed { get; set; }
    }

}

