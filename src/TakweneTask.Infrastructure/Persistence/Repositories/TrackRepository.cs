using Microsoft.EntityFrameworkCore;
using TakweneTask.Application.Common.Interfaces;
using TakweneTask.Application.Common.Models;
using TakweneTask.Domain.Entities;
using TakweneTask.Domain.Enums;

namespace TakweneTask.Infrastructure.Persistence.Repositories;

public class TrackRepository : Repository<Track>, ITrackRepository
{
    private readonly IUnitOfWork _unitOfWork;

    public TrackRepository(AppDbContext dbContext, IUnitOfWork unitOfWork) : base(dbContext)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Track?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(t => t.Artist)
            .Include(t => t.Distributions)
                .ThenInclude(d => d.Dsp)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Track?> GetByIdWithDistributionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Distributions)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Track>> GetAllAsync(
        Guid? artistId = null,
        string? genre = null,
        TrackStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilterQuery(artistId, genre, status);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<Track>> GetPagedAsync(
        Guid? artistId = null,
        string? genre = null,
        TrackStatus? status = null,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilterQuery(artistId, genre, status);
        return await _unitOfWork.PaginateAsync(query, pagination ?? new PaginationParams(), cancellationToken);
    }

    private IQueryable<Track> BuildFilterQuery(Guid? artistId, string? genre, TrackStatus? status)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(t => t.Artist)
            .AsQueryable();

        if (artistId.HasValue)
        {
            query = query.Where(t => t.ArtistId == artistId.Value);
        }

        if (!string.IsNullOrWhiteSpace(genre))
        {
            var trimmedGenre = genre.Trim().ToLower();
            query = query.Where(t => t.Genre.ToLower().Contains(trimmedGenre));
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        return query.OrderByDescending(t => t.ReleaseDate);
    }
}
