using TakweneTask.Application.Common.Models;
using TakweneTask.Application.DTOs.Artists;

namespace TakweneTask.Application.Services.Artists;

public interface IArtistService
{
    Task<Result<ArtistResponse>> CreateAsync(CreateArtistRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ArtistResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
}
