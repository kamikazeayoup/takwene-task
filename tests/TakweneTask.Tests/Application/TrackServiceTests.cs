using AutoMapper;
using TakweneTask.Application.Common.Models;
using TakweneTask.Application.DTOs.Tracks;
using TakweneTask.Application.Mappings;
using TakweneTask.Application.Services.Tracks;
using TakweneTask.Domain.Entities;
using TakweneTask.Domain.Enums;
using TakweneTask.Infrastructure.Persistence;
using TakweneTask.Tests.Common;

namespace TakweneTask.Tests.Application;

public class TrackServiceTests
{
    private readonly IMapper _mapper;

    public TrackServiceTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task CreateAsync_SavesTrackAndResolvesArtistViaUnitOfWork()
    {
        using var db = TestDbContextFactory.Create();
        var uow = new UnitOfWork(db);
        var service = new TrackService(uow, _mapper);

        var artist = new Artist(Guid.NewGuid(), "Fairouz", "fairouz@example.com", "LB");
        await uow.Artists.AddAsync(artist);
        await uow.SaveChangesAsync();

        var request = new CreateTrackRequest
        {
            Title = "Li Beirut",
            ArtistId = artist.Id,
            Isrc = "LB-A01-26-00001",
            ReleaseDate = new DateOnly(2026, 5, 1),
            Genre = "Classic Arabic"
        };

        var result = await service.CreateAsync(request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Li Beirut", result.Value.Title);
        Assert.Equal("Fairouz", result.Value.ArtistName);
        Assert.Equal("LBA012600001", result.Value.Isrc);
        Assert.Equal("draft", result.Value.Status);

        var savedTrack = await uow.Tracks.GetByIdAsync(result.Value.Id);
        Assert.NotNull(savedTrack);
        Assert.Equal("Li Beirut", savedTrack.Title);
    }

    [Fact]
    public async Task CreateAsync_NonExistentArtist_ReturnsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var uow = new UnitOfWork(db);
        var service = new TrackService(uow, _mapper);

        var request = new CreateTrackRequest
        {
            Title = "Unknown Song",
            ArtistId = Guid.NewGuid(),
            Isrc = "EGA012688888",
            ReleaseDate = new DateOnly(2026, 5, 1),
            Genre = "Pop"
        };

        var result = await service.CreateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task GetAllAsync_And_GetPagedAsync_ApplyFiltersAndPagination()
    {
        using var db = TestDbContextFactory.Create();
        var uow = new UnitOfWork(db);
        var service = new TrackService(uow, _mapper);

        var artist1 = new Artist(Guid.NewGuid(), "Artist One", "one@example.com", "EG");
        var artist2 = new Artist(Guid.NewGuid(), "Artist Two", "two@example.com", "EG");
        await uow.Artists.AddRangeAsync(new[] { artist1, artist2 });
        await uow.SaveChangesAsync();

        for (int i = 1; i <= 8; i++)
        {
            var track = new Track(
                Guid.NewGuid(),
                $"Track {i:D2}",
                i <= 5 ? artist1.Id : artist2.Id,
                $"EGA0126000{i:D2}",
                new DateOnly(2026, 1, i),
                i % 2 == 0 ? "Rock" : "Pop"
            );
            await uow.Tracks.AddAsync(track);
        }
        await uow.SaveChangesAsync();

        // 1. Filter by artist1: should return 5 tracks
        var artist1Tracks = await service.GetAllAsync(new GetTracksRequest { ArtistId = artist1.Id });
        Assert.True(artist1Tracks.IsSuccess);
        Assert.Equal(5, artist1Tracks.Value!.Items.Count);
        Assert.Equal(5, artist1Tracks.Value.TotalCount);

        // 2. Filter by genre "Rock": should return 4 tracks
        var rockTracks = await service.GetAllAsync(new GetTracksRequest { Genre = "Rock" });
        Assert.True(rockTracks.IsSuccess);
        Assert.Equal(4, rockTracks.Value!.Items.Count);
        Assert.Equal(4, rockTracks.Value.TotalCount);

        // 3. Paginated query: Page 1 with size 3
        var pagedResult = await service.GetAllAsync(new GetTracksRequest { PageNumber = 1, PageSize = 3 });
        Assert.True(pagedResult.IsSuccess);
        Assert.Equal(3, pagedResult.Value!.Items.Count);
        Assert.Equal(8, pagedResult.Value.TotalCount);
        Assert.Equal(3, pagedResult.Value.TotalPages);
        Assert.True(pagedResult.Value.HasNextPage);
        Assert.False(pagedResult.Value.HasPreviousPage);

        // 4. Paginated query with filter: artist1 and genre "Pop"
        var filteredPaged = await service.GetAllAsync(new GetTracksRequest
        {
            ArtistId = artist1.Id,
            Genre = "Pop",
            PageNumber = 1,
            PageSize = 10
        });
        Assert.True(filteredPaged.IsSuccess);
        Assert.Equal(3, filteredPaged.Value!.Items.Count); // tracks 1, 3, 5
        Assert.Equal(3, filteredPaged.Value.TotalCount);

        // 5. Filter by status: should return 8 draft tracks
        var draftTracks = await service.GetAllAsync(new GetTracksRequest { Status = TrackStatus.Draft });
        Assert.True(draftTracks.IsSuccess);
        Assert.Equal(8, draftTracks.Value!.Items.Count);
    }

    [Fact]
    public async Task DistributeAndLifecycle_TransitionsStateAndPersistsViaUnitOfWork()
    {
        using var db = TestDbContextFactory.Create();
        var uow = new UnitOfWork(db);
        var service = new TrackService(uow, _mapper);

        var artist = new Artist(Guid.NewGuid(), "Wegz", "wegz@example.com", "EG");
        var dsp1 = new Dsp(Guid.NewGuid(), "Spotify");
        var dsp2 = new Dsp(Guid.NewGuid(), "Apple Music");
        await uow.Artists.AddAsync(artist);
        await uow.Dsps.AddRangeAsync(new[] { dsp1, dsp2 });

        var track = new Track(
            Guid.NewGuid(),
            "El Bakht",
            artist.Id,
            "EGA012699999",
            new DateOnly(2026, 3, 1),
            "Trap"
        );
        await uow.Tracks.AddAsync(track);
        await uow.SaveChangesAsync();

        // 1. Distribute to Spotify and Apple Music -> Transitions to Submitted
        var distResult = await service.DistributeAsync(track.Id, new DistributeTrackRequest
        {
            DspIds = new List<Guid> { dsp1.Id, dsp2.Id }
        });

        Assert.True(distResult.IsSuccess);
        Assert.Equal("submitted", distResult.Value!.Status);
        Assert.Equal(2, distResult.Value.Distributions.Count);

        // 2. Attempting to advance to distributed without a Live DSP fails with 409 Conflict
        var prematureStatusResult = await service.UpdateStatusAsync(track.Id, new UpdateTrackStatusRequest
        {
            Status = "distributed"
        });
        Assert.False(prematureStatusResult.IsSuccess);
        Assert.Equal(ErrorType.Conflict, prematureStatusResult.ErrorType);

        // 3. Update Spotify distribution to live
        var updateDspResult = await service.UpdateDistributionStatusAsync(track.Id, dsp1.Id, new UpdateDistributionStatusRequest
        {
            Status = "live"
        });
        Assert.True(updateDspResult.IsSuccess);
        Assert.Contains(updateDspResult.Value!.Distributions, d => d.DspName == "Spotify" && d.Status == "live");

        // 4. Now advance track status to distributed -> succeeds!
        var distributedResult = await service.UpdateStatusAsync(track.Id, new UpdateTrackStatusRequest
        {
            Status = "distributed"
        });
        Assert.True(distributedResult.IsSuccess);
        Assert.Equal("distributed", distributedResult.Value!.Status);

        // 5. Verify persistence in DB via fresh UoW
        var reloadedTrack = await uow.Tracks.GetByIdAsync(track.Id);
        Assert.NotNull(reloadedTrack);
        Assert.Equal(TrackStatus.Distributed, reloadedTrack.Status);
    }
}
