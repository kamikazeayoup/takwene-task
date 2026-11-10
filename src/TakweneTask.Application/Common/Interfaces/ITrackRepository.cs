using TakweneTask.Application.Common.Models;
using TakweneTask.Domain.Entities;
using TakweneTask.Domain.Enums;

namespace TakweneTask.Application.Common.Interfaces;

public interface ITrackRepository : IRepository<Track>
{
    Task<Track?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Track?> GetByIdWithDistributionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Track>> GetAllAsync(
        Guid? artistId = null,
        string? genre = null,
        TrackStatus? status = null,
        CancellationToken cancellationToken = default);
    Task<PagedResult<Track>> GetPagedAsync(
        Guid? artistId = null,
        string? genre = null,
        TrackStatus? status = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default);
}
