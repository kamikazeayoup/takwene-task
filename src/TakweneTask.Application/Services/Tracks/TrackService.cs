using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TakweneTask.Application.Common.Exceptions;
using TakweneTask.Application.Common.Interfaces;
using TakweneTask.Application.DTOs.Tracks;
using TakweneTask.Domain.Entities;
using TakweneTask.Domain.Enums;
using TakweneTask.Domain.Helpers;

namespace TakweneTask.Application.Services.Tracks;

public class TrackService : ITrackService
{
    private readonly IAppDbContext _dbContext;
    private readonly IMapper _mapper;

    public TrackService(IAppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<TrackSummaryResponse> CreateAsync(CreateTrackRequest request, CancellationToken cancellationToken = default)
    {
        var track = new Track(
            Guid.CreateVersion7(),
            request.Title.Trim(),
            request.ArtistId,
            Isrc.Normalize(request.Isrc),
            request.ReleaseDate,
            request.Genre.Trim()
        );

        _dbContext.Tracks.Add(track);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Load artist navigation for response projection
        await _dbContext.Artists
            .Where(a => a.Id == track.ArtistId)
            .LoadAsync(cancellationToken);

        return _mapper.Map<TrackSummaryResponse>(track);
    }

    public async Task<IReadOnlyList<TrackSummaryResponse>> GetAllAsync(
        Guid? artistId,
        string? genre,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Tracks
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

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (Enum.TryParse<TrackStatus>(status, true, out var trackStatus))
            {
                query = query.Where(t => t.Status == trackStatus);
            }
        }

        var tracks = await query
            .OrderByDescending(t => t.ReleaseDate)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<TrackSummaryResponse>>(tracks);
    }

    public async Task<TrackDetailResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var track = await _dbContext.Tracks
            .AsNoTracking()
            .Include(t => t.Artist)
            .Include(t => t.Distributions)
                .ThenInclude(d => d.Dsp)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (track == null)
        {
            throw new NotFoundException("Track", id);
        }

        return _mapper.Map<TrackDetailResponse>(track);
    }

    public async Task<TrackDetailResponse> DistributeAsync(Guid id, DistributeTrackRequest request, CancellationToken cancellationToken = default)
    {
        var track = await _dbContext.Tracks
            .Include(t => t.Distributions)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (track == null)
        {
            throw new NotFoundException("Track", id);
        }

        track.Distribute(request.DspIds, DateTime.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<TrackDetailResponse> UpdateStatusAsync(Guid id, UpdateTrackStatusRequest request, CancellationToken cancellationToken = default)
    {
        var track = await _dbContext.Tracks
            .Include(t => t.Distributions)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (track == null)
        {
            throw new NotFoundException("Track", id);
        }

        var targetStatus = Enum.Parse<TrackStatus>(request.Status, true);
        track.ChangeStatus(targetStatus);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<TrackDetailResponse> UpdateDistributionStatusAsync(
        Guid id,
        Guid dspId,
        UpdateDistributionStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var track = await _dbContext.Tracks
            .Include(t => t.Distributions)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (track == null)
        {
            throw new NotFoundException("Track", id);
        }

        var targetStatus = Enum.Parse<DistributionStatus>(request.Status, true);
        track.SetDistributionStatus(dspId, targetStatus);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }
}
