using TakweneTask.Application.DTOs.Tracks;
using TakweneTask.Application.Validators.Tracks;
using TakweneTask.Domain.Entities;
using TakweneTask.Tests.Common;

namespace TakweneTask.Tests.Application;

public class DistributeTrackRequestValidatorTests
{
    [Fact]
    public async Task ValidRequest_PassesValidation()
    {
        using var db = TestDbContextFactory.Create();
        var dsp1 = new Dsp(Guid.NewGuid(), "Spotify");
        var dsp2 = new Dsp(Guid.NewGuid(), "Apple Music");
        db.Dsps.AddRange(dsp1, dsp2);
        await db.SaveChangesAsync();

        var validator = new DistributeTrackRequestValidator(TestDbContextFactory.CreateUnitOfWork(db));
        var request = new DistributeTrackRequest
        {
            DspIds = new List<Guid> { dsp1.Id, dsp2.Id }
        };

        var result = await validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task EmptyDspList_FailsValidation()
    {
        using var db = TestDbContextFactory.Create();
        var validator = new DistributeTrackRequestValidator(TestDbContextFactory.CreateUnitOfWork(db));
        var request = new DistributeTrackRequest
        {
            DspIds = new List<Guid>()
        };

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(DistributeTrackRequest.DspIds));
    }

    [Fact]
    public async Task DuplicateDspIds_FailsValidation()
    {
        using var db = TestDbContextFactory.Create();
        var dsp1 = new Dsp(Guid.NewGuid(), "Spotify");
        db.Dsps.Add(dsp1);
        await db.SaveChangesAsync();

        var validator = new DistributeTrackRequestValidator(TestDbContextFactory.CreateUnitOfWork(db));
        var request = new DistributeTrackRequest
        {
            DspIds = new List<Guid> { dsp1.Id, dsp1.Id }
        };

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Duplicate DSP IDs are not allowed"));
    }

    [Fact]
    public async Task NonExistentDspId_FailsValidation()
    {
        using var db = TestDbContextFactory.Create();
        var dsp1 = new Dsp(Guid.NewGuid(), "Spotify");
        db.Dsps.Add(dsp1);
        await db.SaveChangesAsync();

        var validator = new DistributeTrackRequestValidator(TestDbContextFactory.CreateUnitOfWork(db));
        var request = new DistributeTrackRequest
        {
            DspIds = new List<Guid> { dsp1.Id, Guid.NewGuid() }
        };

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("do not exist"));
    }
}
