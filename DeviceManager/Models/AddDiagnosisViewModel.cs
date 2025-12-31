namespace DeviceManager.Models
{
    public class AddDiagnosisViewModel
    {
        public int DeviceId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Recommendation { get; set; }
    }
}
