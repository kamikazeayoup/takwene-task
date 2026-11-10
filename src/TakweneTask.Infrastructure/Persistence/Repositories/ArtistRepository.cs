using Microsoft.EntityFrameworkCore;
using TakweneTask.Application.Common.Interfaces;
using TakweneTask.Application.Common.Models;
using TakweneTask.Domain.Entities;

namespace TakweneTask.Infrastructure.Persistence.Repositories;

public class ArtistRepository : Repository<Artist>, IArtistRepository
{
    private readonly IUnitOfWork _unitOfWork;

    public ArtistRepository(AppDbContext dbContext, IUnitOfWork unitOfWork) : base(dbContext)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<Artist>> GetAllAsync(PaginationParams? pagination = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .OrderBy(a => a.Name);

        return await _unitOfWork.PaginateAsync(query, pagination, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(a => a.Id == id, cancellationToken);
    }
}
