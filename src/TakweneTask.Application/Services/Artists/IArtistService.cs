using TakweneTask.Application.DTOs.Artists;

namespace TakweneTask.Application.Services.Artists;

public interface IArtistService
{
    Task<ArtistResponse> CreateAsync(CreateArtistRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ArtistResponse>> GetAllAsync(CancellationToken cancellationToken = default);
}
