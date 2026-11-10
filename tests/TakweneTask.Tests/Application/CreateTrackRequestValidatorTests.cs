using TakweneTask.Application.DTOs.Tracks;
using TakweneTask.Application.Validators.Tracks;
using TakweneTask.Domain.Entities;
using TakweneTask.Tests.Common;

namespace TakweneTask.Tests.Application;

public class CreateTrackRequestValidatorTests
{
    [Fact]
    public async Task ValidRequest_PassesValidation()
    {
        using var db = TestDbContextFactory.Create();
        var artist = new Artist(Guid.NewGuid(), "Nour", "nour@test.com", "EG");
        db.Artists.Add(artist);
        await db.SaveChangesAsync();

        var validator = new CreateTrackRequestValidator(db);
        var request = new CreateTrackRequest
        {
            Title = "Habibi Da",
            ArtistId = artist.Id,
            Isrc = "USRC17607839",
            ReleaseDate = new DateOnly(2026, 11, 10),
            Genre = "Arabic Pop"
        };

        var result = await validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task NonExistentArtistId_FailsValidation()
    {
        using var db = TestDbContextFactory.Create();
        var validator = new CreateTrackRequestValidator(db);
        var request = new CreateTrackRequest
        {
            Title = "Habibi Da",
            ArtistId = Guid.NewGuid(),
            Isrc = "USRC17607839",
            ReleaseDate = new DateOnly(2026, 11, 10),
            Genre = "Arabic Pop"
        };

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTrackRequest.ArtistId));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task EmptyTitle_FailsValidation(string? title)
    {
        using var db = TestDbContextFactory.Create();
        var validator = new CreateTrackRequestValidator(db);
        var request = new CreateTrackRequest
        {
            Title = title!,
            ArtistId = Guid.NewGuid(),
            Isrc = "USRC17607839",
            ReleaseDate = new DateOnly(2026, 11, 10),
            Genre = "Arabic Pop"
        };

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTrackRequest.Title));
    }

    [Theory]
    [InlineData("invalid-isrc")]
    [InlineData("US12345678901")] // 13 chars
    [InlineData("12RC17607839")] // country code not letters
    [InlineData("USRC1760783")] // 11 chars
    public async Task InvalidIsrc_FailsValidation(string isrc)
    {
        using var db = TestDbContextFactory.Create();
        var validator = new CreateTrackRequestValidator(db);
        var request = new CreateTrackRequest
        {
            Title = "Valid Title",
            ArtistId = Guid.NewGuid(),
            Isrc = isrc,
            ReleaseDate = new DateOnly(2026, 11, 10),
            Genre = "Pop"
        };

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTrackRequest.Isrc));
    }

    [Fact]
    public async Task ValidIsrcWithHyphens_PassesValidation()
    {
        using var db = TestDbContextFactory.Create();
        var artist = new Artist(Guid.NewGuid(), "Nour", "nour@test.com", "EG");
        db.Artists.Add(artist);
        await db.SaveChangesAsync();

        var validator = new CreateTrackRequestValidator(db);
        var request = new CreateTrackRequest
        {
            Title = "Habibi Da",
            ArtistId = artist.Id,
            Isrc = "US-RC1-76-07839",
            ReleaseDate = new DateOnly(2026, 11, 10),
            Genre = "Arabic Pop"
        };

        var result = await validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }
}
