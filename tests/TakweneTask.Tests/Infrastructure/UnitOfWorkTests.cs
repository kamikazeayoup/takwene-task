using TakweneTask.Application.Common.Models;
using TakweneTask.Domain.Entities;
using TakweneTask.Domain.Enums;
using TakweneTask.Infrastructure.Persistence;
using TakweneTask.Tests.Common;

namespace TakweneTask.Tests.Infrastructure;

public class UnitOfWorkTests
{
    [Fact]
    public async Task SaveChangesAsync_PersistsEntitiesToDatabase()
    {
        using var db = TestDbContextFactory.Create();
        var uow = new UnitOfWork(db);

        var artist = new Artist(Guid.NewGuid(), "Tamer Hosny", "tamer@example.com", "EG");
        await uow.Artists.AddAsync(artist);

        Assert.True(uow.HasChanges());

        var rowsAffected = await uow.SaveChangesAsync();

        Assert.Equal(1, rowsAffected);
        Assert.False(uow.HasChanges());

        var fetched = await uow.Artists.GetByIdAsync(artist.Id);
        Assert.NotNull(fetched);
        Assert.Equal("Tamer Hosny", fetched.Name);
    }

    [Fact]
    public async Task PaginateAsync_WithoutPagination_ReturnsAllRecordsInPagedEnvelope()
    {
        using var db = TestDbContextFactory.Create();
        var uow = new UnitOfWork(db);

        for (int i = 1; i <= 5; i++)
        {
            await uow.Artists.AddAsync(new Artist(Guid.NewGuid(), $"Artist {i}", $"art{i}@test.com", "EG"));
        }
        await uow.SaveChangesAsync();

        var query = uow.Artists.Query().OrderBy(a => a.Name);

        var result = await uow.PaginateAsync(query, paginationParams: null);

        Assert.Equal(5, result.Items.Count);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(1, result.PageNumber);
        Assert.False(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public async Task PaginateAsync_ReturnsCorrectPageAndMetadata()
    {
        using var db = TestDbContextFactory.Create();
        var uow = new UnitOfWork(db);

        // Add 15 artists
        for (int i = 1; i <= 15; i++)
        {
            await uow.Artists.AddAsync(new Artist(
                Guid.NewGuid(),
                $"Artist {i:D2}",
                $"artist{i}@example.com",
                "EG"
            ));
        }
        await uow.SaveChangesAsync();

        var query = uow.Artists.Query().OrderBy(a => a.Name);

        // Page 1 with pageSize 5
        var page1 = await uow.PaginateAsync(query, 1, 5);
        Assert.Equal(5, page1.Items.Count);
        Assert.Equal(15, page1.TotalCount);
        Assert.Equal(3, page1.TotalPages);
        Assert.Equal(1, page1.PageNumber);
        Assert.Equal(5, page1.PageSize);
        Assert.False(page1.HasPreviousPage);
        Assert.True(page1.HasNextPage);
        Assert.Equal("Artist 01", page1.Items[0].Name);
        Assert.Equal("Artist 05", page1.Items[4].Name);

        // Page 2 with pageSize 5
        var page2 = await uow.PaginateAsync(query, 2, 5);
        Assert.Equal(5, page2.Items.Count);
        Assert.Equal(2, page2.PageNumber);
        Assert.True(page2.HasPreviousPage);
        Assert.True(page2.HasNextPage);
        Assert.Equal("Artist 06", page2.Items[0].Name);

        // Page 3 with pageSize 5 (last page)
        var page3 = await uow.PaginateAsync(query, 3, 5);
        Assert.Equal(5, page3.Items.Count);
        Assert.Equal(3, page3.PageNumber);
        Assert.True(page3.HasPreviousPage);
        Assert.False(page3.HasNextPage);
        Assert.Equal("Artist 11", page3.Items[0].Name);
        Assert.Equal("Artist 15", page3.Items[4].Name);
    }

    [Fact]
    public async Task PaginateAsync_HandlesBoundaryAndInvalidInputsGracefully()
    {
        using var db = TestDbContextFactory.Create();
        var uow = new UnitOfWork(db);

        await uow.Artists.AddAsync(new Artist(Guid.NewGuid(), "Sherine", "sherine@example.com", "EG"));
        await uow.SaveChangesAsync();

        var query = uow.Artists.Query();

        // Page number < 1 should default to 1, page size < 1 should default to 10
        var result = await uow.PaginateAsync(query, -5, -20);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Single(result.Items);

        // Page size > 100 should be clamped to 100
        var largePage = await uow.PaginateAsync(query, new PaginationParams(1, 500));
        Assert.Equal(100, largePage.PageSize);

        // Out-of-bounds page should return empty items with correct TotalCount
        var outOfBounds = await uow.PaginateAsync(query, 10, 10);
        Assert.Empty(outOfBounds.Items);
        Assert.Equal(1, outOfBounds.TotalCount);
        Assert.Equal(1, outOfBounds.TotalPages);
        Assert.True(outOfBounds.HasPreviousPage);
        Assert.False(outOfBounds.HasNextPage);
    }

    [Fact]
    public void Repositories_AreCachedAndReusedOnSubsequentCalls()
    {
        using var db = TestDbContextFactory.Create();
        var uow = new UnitOfWork(db);

        var artists1 = uow.Artists;
        var artists2 = uow.Artists;
        Assert.Same(artists1, artists2);

        var tracks1 = uow.Tracks;
        var tracks2 = uow.Tracks;
        Assert.Same(tracks1, tracks2);

        var dsps1 = uow.Dsps;
        var dsps2 = uow.Dsps;
        Assert.Same(dsps1, dsps2);

        var genericRepo1 = uow.Repository<Artist>();
        var genericRepo2 = uow.Repository<Artist>();
        Assert.Same(genericRepo1, genericRepo2);
    }

    [Fact]
    public async Task ChangeTracker_ClearAndDetach_WorkCorrectly()
    {
        using var db = TestDbContextFactory.Create();
        var uow = new UnitOfWork(db);

        var artist = new Artist(Guid.NewGuid(), "Mohamed Mounir", "mounir@example.com", "EG");
        await uow.Artists.AddAsync(artist);
        Assert.True(uow.HasChanges());

        uow.ClearChangeTracker();
        Assert.False(uow.HasChanges());

        await uow.Artists.AddAsync(artist);
        Assert.True(uow.HasChanges());

        uow.Detach(artist);
        Assert.False(uow.HasChanges());
    }
}
