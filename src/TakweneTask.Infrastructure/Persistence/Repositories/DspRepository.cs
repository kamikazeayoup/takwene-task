using Microsoft.EntityFrameworkCore;
using TakweneTask.Application.Common.Interfaces;
using TakweneTask.Domain.Entities;

namespace TakweneTask.Infrastructure.Persistence.Repositories;

public class DspRepository : Repository<Dsp>, IDspRepository
{
    public DspRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<int> CountByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids as IReadOnlyCollection<Guid> ?? ids.ToList();
        return await _dbSet.CountAsync(d => idList.Contains(d.Id), cancellationToken);
    }

    public async Task<bool> AllExistAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids as IReadOnlyCollection<Guid> ?? ids.ToList();
        if (idList.Count == 0) return true;
        var distinctCount = idList.Distinct().Count();
        var countInDb = await CountByIdsAsync(idList, cancellationToken);
        return countInDb == distinctCount;
    }
}
