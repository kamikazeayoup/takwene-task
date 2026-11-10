using TakweneTask.Domain.Entities;
using TakweneTask.Domain.Enums;
using TakweneTask.Domain.Exceptions;

namespace TakweneTask.Domain.States;

public class DistributedTrackState : ITrackState
{
    public TrackStatus Status => TrackStatus.Distributed;

    public void Distribute(Track track, IReadOnlyCollection<Guid> dspIds, DateTime utcNow)
    {
        if (dspIds == null || dspIds.Count == 0)
        {
            throw new DomainException("At least one DSP must be specified for distribution.");
        }

        // All-or-nothing validation first
        foreach (var dspId in dspIds)
        {
            var existing = track.Distributions.FirstOrDefault(d => d.DspId == dspId);
            if (existing != null && (existing.Status == DistributionStatus.Pending || existing.Status == DistributionStatus.Live))
            {
                throw new DomainException($"Track is already pending or live on DSP '{dspId}'.");
            }
        }

        // Apply mutations
        foreach (var dspId in dspIds)
        {
            var existing = track.Distributions.FirstOrDefault(d => d.DspId == dspId);
            if (existing != null && existing.Status == DistributionStatus.Rejected)
            {
                existing.Resubmit(utcNow);
            }
            else if (existing == null)
            {
                track.AddDistribution(new TrackDistribution(Guid.CreateVersion7(), track.Id, dspId, utcNow, DistributionStatus.Pending));
            }
        }
    }

    public void ChangeStatus(Track track, TrackStatus newStatus)
    {
        throw new DomainException("Track is already distributed. Status changes from distributed are not permitted.");
    }
}
