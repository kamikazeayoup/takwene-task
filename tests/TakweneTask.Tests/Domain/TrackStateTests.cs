using TakweneTask.Domain.Entities;
using TakweneTask.Domain.Enums;
using TakweneTask.Domain.Exceptions;

namespace TakweneTask.Tests.Domain;

public class TrackStateTests
{
    private readonly Guid _artistId = Guid.NewGuid();
    private readonly DateTime _utcNow = new(2026, 11, 10, 12, 0, 0, DateTimeKind.Utc);

    private Track CreateDraftTrack()
    {
        return new Track(
            Guid.CreateVersion7(),
            "Salam Ya Sahbi",
            _artistId,
            "EGA012600001",
            new DateOnly(2026, 11, 10),
            "Mahraganat"
        );
    }

    [Fact]
    public void Constructor_InitializesTrackInDraftStatus()
    {
        var track = CreateDraftTrack();

        Assert.Equal(TrackStatus.Draft, track.Status);
        Assert.Empty(track.Distributions);
    }

    [Fact]
    public void Distribute_FromDraft_TransitionsToSubmitted_AndAddsPendingDistributions()
    {
        var track = CreateDraftTrack();
        var dsp1 = Guid.NewGuid();
        var dsp2 = Guid.NewGuid();

        track.Distribute(new[] { dsp1, dsp2 }, _utcNow);

        Assert.Equal(TrackStatus.Submitted, track.Status);
        Assert.Equal(2, track.Distributions.Count);
        Assert.All(track.Distributions, d =>
        {
            Assert.Equal(DistributionStatus.Pending, d.Status);
            Assert.Equal(_utcNow, d.SubmittedAt);
        });
    }

    [Fact]
    public void Distribute_WithEmptyDspIds_ThrowsDomainException()
    {
        var track = CreateDraftTrack();

        var ex = Assert.Throws<DomainException>(() => track.Distribute(Array.Empty<Guid>(), _utcNow));
        Assert.Contains("At least one DSP must be specified", ex.Message);
    }

    [Fact]
    public void ChangeStatus_FromDraft_ThrowsDomainException()
    {
        var track = CreateDraftTrack();

        var ex = Assert.Throws<DomainException>(() => track.ChangeStatus(TrackStatus.Submitted));
        Assert.Contains("Cannot change status directly from draft", ex.Message);

        var ex2 = Assert.Throws<DomainException>(() => track.ChangeStatus(TrackStatus.Distributed));
        Assert.Contains("Cannot change status directly from draft", ex2.Message);
    }

    [Fact]
    public void Distribute_FromSubmitted_AddsNewDsp_AndResubmitsRejectedDsp()
    {
        var track = CreateDraftTrack();
        var dsp1 = Guid.NewGuid();
        var dsp2 = Guid.NewGuid();
        track.Distribute(new[] { dsp1, dsp2 }, _utcNow);

        // Simulate dsp2 rejected
        track.SetDistributionStatus(dsp2, DistributionStatus.Rejected);

        var dsp3 = Guid.NewGuid();
        var newUtcNow = _utcNow.AddHours(2);
        track.Distribute(new[] { dsp2, dsp3 }, newUtcNow);

        Assert.Equal(TrackStatus.Submitted, track.Status);
        Assert.Equal(3, track.Distributions.Count);

        var dist2 = track.Distributions.First(d => d.DspId == dsp2);
        Assert.Equal(DistributionStatus.Pending, dist2.Status);
        Assert.Equal(newUtcNow, dist2.SubmittedAt);

        var dist3 = track.Distributions.First(d => d.DspId == dsp3);
        Assert.Equal(DistributionStatus.Pending, dist3.Status);
        Assert.Equal(newUtcNow, dist3.SubmittedAt);
    }

    [Fact]
    public void Distribute_WithExistingPendingOrLiveDsp_ThrowsDomainException_WithAllOrNothingSemantics()
    {
        var track = CreateDraftTrack();
        var dsp1 = Guid.NewGuid();
        track.Distribute(new[] { dsp1 }, _utcNow);

        var dsp2 = Guid.NewGuid();
        // dsp1 is already Pending, dsp2 is new
        var ex = Assert.Throws<DomainException>(() => track.Distribute(new[] { dsp2, dsp1 }, _utcNow));
        Assert.Contains("already pending or live", ex.Message);

        // Verify all-or-nothing: dsp2 must NOT have been added
        Assert.Single(track.Distributions);
        Assert.DoesNotContain(track.Distributions, d => d.DspId == dsp2);
    }

    [Fact]
    public void ChangeStatus_ToDistributed_WithoutLiveDsp_ThrowsDomainException()
    {
        var track = CreateDraftTrack();
        var dsp1 = Guid.NewGuid();
        track.Distribute(new[] { dsp1 }, _utcNow);

        var ex = Assert.Throws<DomainException>(() => track.ChangeStatus(TrackStatus.Distributed));
        Assert.Contains("at least one DSP distribution is live", ex.Message);
        Assert.Equal(TrackStatus.Submitted, track.Status);
    }

    [Fact]
    public void ChangeStatus_ToDistributed_WithAtLeastOneLiveDsp_TransitionsToDistributed()
    {
        var track = CreateDraftTrack();
        var dsp1 = Guid.NewGuid();
        var dsp2 = Guid.NewGuid();
        track.Distribute(new[] { dsp1, dsp2 }, _utcNow);

        track.SetDistributionStatus(dsp1, DistributionStatus.Live);
        track.ChangeStatus(TrackStatus.Distributed);

        Assert.Equal(TrackStatus.Distributed, track.Status);
    }

    [Fact]
    public void ChangeStatus_FromSubmitted_ToDraft_ThrowsDomainException()
    {
        var track = CreateDraftTrack();
        track.Distribute(new[] { Guid.NewGuid() }, _utcNow);

        var ex = Assert.Throws<DomainException>(() => track.ChangeStatus(TrackStatus.Draft));
        Assert.Contains("Cannot change status from submitted to draft", ex.Message);
    }

    [Fact]
    public void ChangeStatus_FromDistributed_ThrowsDomainException()
    {
        var track = CreateDraftTrack();
        var dsp1 = Guid.NewGuid();
        track.Distribute(new[] { dsp1 }, _utcNow);
        track.SetDistributionStatus(dsp1, DistributionStatus.Live);
        track.ChangeStatus(TrackStatus.Distributed);

        var ex = Assert.Throws<DomainException>(() => track.ChangeStatus(TrackStatus.Submitted));
        Assert.Contains("Track is already distributed. Status changes from distributed are not permitted.", ex.Message);
    }

    [Fact]
    public void Distribute_FromDistributed_AllowsNewDsps()
    {
        var track = CreateDraftTrack();
        var dsp1 = Guid.NewGuid();
        track.Distribute(new[] { dsp1 }, _utcNow);
        track.SetDistributionStatus(dsp1, DistributionStatus.Live);
        track.ChangeStatus(TrackStatus.Distributed);

        var dsp2 = Guid.NewGuid();
        track.Distribute(new[] { dsp2 }, _utcNow.AddDays(1));

        Assert.Equal(TrackStatus.Distributed, track.Status);
        Assert.Equal(2, track.Distributions.Count);
        var dist2 = track.Distributions.First(d => d.DspId == dsp2);
        Assert.Equal(DistributionStatus.Pending, dist2.Status);
    }

    [Fact]
    public void SetDistributionStatus_OnUnknownDsp_ThrowsDomainException()
    {
        var track = CreateDraftTrack();
        var dsp1 = Guid.NewGuid();
        track.Distribute(new[] { dsp1 }, _utcNow);

        var unknownDsp = Guid.NewGuid();
        var ex = Assert.Throws<DomainException>(() => track.SetDistributionStatus(unknownDsp, DistributionStatus.Live));
        Assert.Contains("not found on track", ex.Message);
    }
}
