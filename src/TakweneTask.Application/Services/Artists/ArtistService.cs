using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TakweneTask.Application.Common.Interfaces;
using TakweneTask.Application.Common.Models;
using TakweneTask.Application.DTOs.Artists;
using TakweneTask.Domain.Entities;

namespace TakweneTask.Application.Services.Artists;

public class ArtistService : IArtistService
{
    private readonly IAppDbContext _dbContext;
    private readonly IMapper _mapper;

    public ArtistService(IAppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<Result<ArtistResponse>> CreateAsync(CreateArtistRequest request, CancellationToken cancellationToken = default)
    {
        var artist = new Artist(
            Guid.CreateVersion7(),
            request.Name.Trim(),
            request.Email.Trim().ToLowerInvariant(),
            request.Country.Trim().ToUpperInvariant()
        );

        _dbContext.Artists.Add(artist);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = _mapper.Map<ArtistResponse>(artist);
        return Result<ArtistResponse>.Success(response);
    }

    public async Task<Result<IReadOnlyList<ArtistResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var artists = await _dbContext.Artists
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);

        var response = _mapper.Map<IReadOnlyList<ArtistResponse>>(artists);
        return Result<IReadOnlyList<ArtistResponse>>.Success(response);
    }
}
