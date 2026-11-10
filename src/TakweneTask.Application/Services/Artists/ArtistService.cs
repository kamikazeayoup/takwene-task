using AutoMapper;
using TakweneTask.Application.Common.Interfaces;
using TakweneTask.Application.Common.Models;
using TakweneTask.Application.DTOs.Artists;
using TakweneTask.Domain.Entities;

namespace TakweneTask.Application.Services.Artists;

public class ArtistService : IArtistService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ArtistService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
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

        await _unitOfWork.Artists.AddAsync(artist, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = _mapper.Map<ArtistResponse>(artist);
        return Result<ArtistResponse>.Success(response);
    }

    public async Task<Result<IReadOnlyList<ArtistResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var artists = await _unitOfWork.Artists.GetAllAsync(cancellationToken);
        var response = _mapper.Map<IReadOnlyList<ArtistResponse>>(artists);
        return Result<IReadOnlyList<ArtistResponse>>.Success(response);
    }

    public async Task<Result<PagedResult<ArtistResponse>>> GetPagedAsync(PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var pagedArtists = await _unitOfWork.Artists.GetPagedAsync(pagination, cancellationToken);
        var mappedItems = _mapper.Map<IReadOnlyList<ArtistResponse>>(pagedArtists.Items);
        var response = new PagedResult<ArtistResponse>(
            mappedItems,
            pagedArtists.TotalCount,
            pagedArtists.PageNumber,
            pagedArtists.PageSize);

        return Result<PagedResult<ArtistResponse>>.Success(response);
    }
}
