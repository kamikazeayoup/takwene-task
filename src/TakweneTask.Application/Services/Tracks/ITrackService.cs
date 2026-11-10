using TakweneTask.Application.Common.Models;
using TakweneTask.Application.DTOs.Tracks;

namespace TakweneTask.Application.Services.Tracks;

public interface ITrackService
{
    Task<Result<TrackSummaryResponse>> CreateAsync(CreateTrackRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<TrackSummaryResponse>>> GetAllAsync(Guid? artistId, string? genre, string? status, CancellationToken cancellationToken = default);
    Task<Result<TrackDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<TrackDetailResponse>> DistributeAsync(Guid id, DistributeTrackRequest request, CancellationToken cancellationToken = default);
    Task<Result<TrackDetailResponse>> UpdateStatusAsync(Guid id, UpdateTrackStatusRequest request, CancellationToken cancellationToken = default);
    Task<Result<TrackDetailResponse>> UpdateDistributionStatusAsync(Guid id, Guid dspId, UpdateDistributionStatusRequest request, CancellationToken cancellationToken = default);
}
