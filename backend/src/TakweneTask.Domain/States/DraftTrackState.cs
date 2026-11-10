using TakweneTask.Domain.Entities;
using TakweneTask.Domain.Enums;
using TakweneTask.Domain.Exceptions;

namespace TakweneTask.Domain.States;

public class DraftTrackState : ITrackState
{
    public TrackStatus Status => TrackStatus.Draft;

    public void Distribute(Track track, IReadOnlyCollection<Guid> dspIds, DateTime utcNow)
    {
        if (dspIds == null || dspIds.Count == 0)
        {
            throw new DomainException("At least one DSP must be specified for distribution.");
        }

        foreach (var dspId in dspIds)
        {
            track.AddDistribution(new TrackDistribution(Guid.CreateVersion7(), track.Id, dspId, utcNow, DistributionStatus.Pending));
        }

        track.SetState(new SubmittedTrackState(), TrackStatus.Submitted);
    }

    public void ChangeStatus(Track track, TrackStatus newStatus)
    {
        throw new DomainException("Cannot change status directly from draft. A track must be distributed to at least one DSP to become submitted.");
    }
}
