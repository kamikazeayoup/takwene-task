using TakweneTask.Domain.Entities;
using TakweneTask.Domain.Enums;

namespace TakweneTask.Domain.States;

public static class TrackStateFactory
{
    public static ITrackState Create(Track track, TrackStatus status) => status switch
    {
        TrackStatus.Draft => new DraftTrackState(),
        TrackStatus.Submitted => new SubmittedTrackState(),
        TrackStatus.Distributed => new DistributedTrackState(),
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, $"Unknown track status: {status}")
    };
}
