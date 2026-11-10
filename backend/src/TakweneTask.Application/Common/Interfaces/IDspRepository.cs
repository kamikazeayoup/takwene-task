using TakweneTask.Domain.Entities;

namespace TakweneTask.Application.Common.Interfaces;

public interface IDspRepository : IRepository<Dsp>
{
    Task<int> CountByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<bool> AllExistAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}
