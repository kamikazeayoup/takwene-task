using TakweneTask.Application.Common.Models;
using TakweneTask.Domain.Entities;

namespace TakweneTask.Application.Common.Interfaces;

public interface IArtistRepository : IRepository<Artist>
{
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<Artist>> GetPagedAsync(PaginationParams pagination, CancellationToken cancellationToken = default);
}
