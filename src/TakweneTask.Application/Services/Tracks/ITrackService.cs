using TakweneTask.Application.DTOs.Tracks;

namespace TakweneTask.Application.Services.Tracks;

public interface ITrackService
{
    Task<TrackSummaryResponse> CreateAsync(CreateTrackRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TrackSummaryResponse>> GetAllAsync(Guid? artistId, string? genre, string? status, CancellationToken cancellationToken = default);
    Task<TrackDetailResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TrackDetailResponse> DistributeAsync(Guid id, DistributeTrackRequest request, CancellationToken cancellationToken = default);
    Task<TrackDetailResponse> UpdateStatusAsync(Guid id, UpdateTrackStatusRequest request, CancellationToken cancellationToken = default);
    Task<TrackDetailResponse> UpdateDistributionStatusAsync(Guid id, Guid dspId, UpdateDistributionStatusRequest request, CancellationToken cancellationToken = default);
}
