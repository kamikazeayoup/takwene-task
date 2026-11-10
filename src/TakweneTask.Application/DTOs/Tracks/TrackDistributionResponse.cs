namespace TakweneTask.Application.DTOs.Tracks;

public class TrackDistributionResponse
{
    public Guid DspId { get; set; }
    public string DspName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}
