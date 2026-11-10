using TakweneTask.Domain.Enums;

namespace TakweneTask.Domain.Entities;

public class TrackDistribution
{
    public Guid Id { get; private set; }
    public Guid TrackId { get; private set; }
    public Track? Track { get; private set; }
    public Guid DspId { get; private set; }
    public Dsp? Dsp { get; private set; }
    public DateTime SubmittedAt { get; private set; }
    public DistributionStatus Status { get; private set; }

    private TrackDistribution()
    {
    }

    public TrackDistribution(Guid id, Guid trackId, Guid dspId, DateTime submittedAt, DistributionStatus status)
    {
        Id = id;
        TrackId = trackId;
        DspId = dspId;
        SubmittedAt = submittedAt;
        Status = status;
    }

    internal void MarkLive()
    {
        Status = DistributionStatus.Live;
    }

    internal void MarkRejected()
    {
        Status = DistributionStatus.Rejected;
    }

    internal void Resubmit(DateTime utcNow)
    {
        Status = DistributionStatus.Pending;
        SubmittedAt = utcNow;
    }
}
