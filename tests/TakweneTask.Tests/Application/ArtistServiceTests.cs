using AutoMapper;
using TakweneTask.Application.Common.Models;
using TakweneTask.Application.DTOs.Artists;
using TakweneTask.Application.Mappings;
using TakweneTask.Application.Services.Artists;
using TakweneTask.Infrastructure.Persistence;
using TakweneTask.Tests.Common;

namespace TakweneTask.Tests.Application;

public class ArtistServiceTests
{
    private readonly IMapper _mapper;

    public ArtistServiceTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task CreateAsync_SavesArtistViaUnitOfWork()
    {
        using var db = TestDbContextFactory.Create();
        var uow = new UnitOfWork(db);
        var service = new ArtistService(uow, _mapper);

        var request = new CreateArtistRequest
        {
            Name = "Angham",
            Email = "angham@example.com",
            Country = "EG"
        };

        var result = await service.CreateAsync(request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Angham", result.Value.Name);
        Assert.Equal("angham@example.com", result.Value.Email);
        Assert.Equal("EG", result.Value.Country);

        var saved = await uow.Artists.GetByIdAsync(result.Value.Id);
        Assert.NotNull(saved);
        Assert.Equal("Angham", saved.Name);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllArtistsOrderedByName()
    {
        using var db = TestDbContextFactory.Create();
        var uow = new UnitOfWork(db);
        var service = new ArtistService(uow, _mapper);

        await service.CreateAsync(new CreateArtistRequest { Name = "Ziad Rahbani", Email = "ziad@example.com", Country = "LB" });
        await service.CreateAsync(new CreateArtistRequest { Name = "Asala", Email = "asala@example.com", Country = "SY" });
        await service.CreateAsync(new CreateArtistRequest { Name = "Kadim Al Sahir", Email = "kadim@example.com", Country = "IQ" });

        var result = await service.GetAllAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.Count);
        Assert.Equal("Asala", result.Value[0].Name);
        Assert.Equal("Kadim Al Sahir", result.Value[1].Name);
        Assert.Equal("Ziad Rahbani", result.Value[2].Name);
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsPagedResultsViaUnitOfWork()
    {
        using var db = TestDbContextFactory.Create();
        var uow = new UnitOfWork(db);
        var service = new ArtistService(uow, _mapper);

        for (int i = 1; i <= 12; i++)
        {
            await service.CreateAsync(new CreateArtistRequest
            {
                Name = $"Artist {i:D2}",
                Email = $"artist{i}@example.com",
                Country = "EG"
            });
        }

        var pagedResult = await service.GetPagedAsync(new PaginationParams(1, 5));

        Assert.True(pagedResult.IsSuccess);
        Assert.Equal(5, pagedResult.Value!.Items.Count);
        Assert.Equal(12, pagedResult.Value.TotalCount);
        Assert.Equal(3, pagedResult.Value.TotalPages);
        Assert.Equal(1, pagedResult.Value.PageNumber);
        Assert.True(pagedResult.Value.HasNextPage);
        Assert.False(pagedResult.Value.HasPreviousPage);

        var page2Result = await service.GetPagedAsync(new PaginationParams(2, 5));
        Assert.True(page2Result.IsSuccess);
        Assert.Equal(5, page2Result.Value!.Items.Count);
        Assert.Equal(2, page2Result.Value.PageNumber);
        Assert.True(page2Result.Value.HasPreviousPage);
        Assert.True(page2Result.Value.HasNextPage);
    }
}
