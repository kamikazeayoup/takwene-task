using TakweneTask.Domain.Enums;
using TakweneTask.Domain.Exceptions;
using TakweneTask.Domain.States;

namespace TakweneTask.Domain.Entities;

public class Track
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = null!;
    public Guid ArtistId { get; private set; }
    public Artist? Artist { get; private set; }
    public string Isrc { get; private set; } = null!;
    public DateOnly ReleaseDate { get; private set; }
    public string Genre { get; private set; } = null!;
    public TrackStatus Status { get; private set; }

    private readonly List<TrackDistribution> _distributions = new();
    public IReadOnlyCollection<TrackDistribution> Distributions => _distributions.AsReadOnly();

    private ITrackState? _state;
    private ITrackState State => _state ??= TrackStateFactory.Create(this, Status);

    private Track()
    {
    }

    public Track(Guid id, string title, Guid artistId, string isrc, DateOnly releaseDate, string genre)
    {
        Id = id;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        ArtistId = artistId;
        Isrc = isrc ?? throw new ArgumentNullException(nameof(isrc));
        ReleaseDate = releaseDate;
        Genre = genre ?? throw new ArgumentNullException(nameof(genre));
        Status = TrackStatus.Draft;
        _state = new DraftTrackState();
    }

    internal void SetState(ITrackState state, TrackStatus status)
    {
        _state = state;
        Status = status;
    }

    internal void AddDistribution(TrackDistribution distribution)
    {
        _distributions.Add(distribution);
    }

    public void Distribute(IReadOnlyCollection<Guid> dspIds, DateTime utcNow)
    {
        State.Distribute(this, dspIds, utcNow);
    }

    public void ChangeStatus(TrackStatus newStatus)
    {
        State.ChangeStatus(this, newStatus);
    }

    public void SetDistributionStatus(Guid dspId, DistributionStatus newStatus)
    {
        var dist = _distributions.FirstOrDefault(d => d.DspId == dspId);
        if (dist == null)
        {
            throw new DomainException($"Distribution for DSP '{dspId}' not found on track '{Id}'.");
        }

        if (newStatus == DistributionStatus.Live)
        {
            dist.MarkLive();
        }
        else if (newStatus == DistributionStatus.Rejected)
        {
            dist.MarkRejected();
        }
        else
        {
            throw new DomainException("Distribution status can only be set to live or rejected.");
        }
    }
}
