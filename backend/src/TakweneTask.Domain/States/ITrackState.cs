using TakweneTask.Domain.Entities;
using TakweneTask.Domain.Enums;

namespace TakweneTask.Domain.States;

public interface ITrackState
{
    TrackStatus Status { get; }
    void Distribute(Track track, IReadOnlyCollection<Guid> dspIds, DateTime utcNow);
    void ChangeStatus(Track track, TrackStatus newStatus);
}
