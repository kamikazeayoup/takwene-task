namespace TakweneTask.Application.DTOs.Tracks;

public class DistributeTrackRequest
{
    public List<Guid> DspIds { get; set; } = new();
}
