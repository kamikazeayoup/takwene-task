using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TakweneTask.Application.Common.Interfaces;
using TakweneTask.Application.Common.Models;
using TakweneTask.Application.DTOs.Tracks;
using TakweneTask.Domain.Entities;
using TakweneTask.Domain.Enums;
using TakweneTask.Domain.Exceptions;
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

    public async Task<Result<TrackSummaryResponse>> CreateAsync(CreateTrackRequest request, CancellationToken cancellationToken = default)
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

        await _dbContext.Artists
            .Where(a => a.Id == track.ArtistId)
            .LoadAsync(cancellationToken);

        var response = _mapper.Map<TrackSummaryResponse>(track);
        return Result<TrackSummaryResponse>.Success(response);
    }

    public async Task<Result<IReadOnlyList<TrackSummaryResponse>>> GetAllAsync(
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

        var response = _mapper.Map<IReadOnlyList<TrackSummaryResponse>>(tracks);
        return Result<IReadOnlyList<TrackSummaryResponse>>.Success(response);
    }

    public async Task<Result<TrackDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var track = await _dbContext.Tracks
            .AsNoTracking()
            .Include(t => t.Artist)
            .Include(t => t.Distributions)
                .ThenInclude(d => d.Dsp)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (track == null)
        {
            return Result<TrackDetailResponse>.NotFound($"Track with ID '{id}' was not found.");
        }

        var response = _mapper.Map<TrackDetailResponse>(track);
        return Result<TrackDetailResponse>.Success(response);
    }

    public async Task<Result<TrackDetailResponse>> DistributeAsync(Guid id, DistributeTrackRequest request, CancellationToken cancellationToken = default)
    {
        var track = await _dbContext.Tracks
            .Include(t => t.Distributions)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (track == null)
        {
            return Result<TrackDetailResponse>.NotFound($"Track with ID '{id}' was not found.");
        }

        try
        {
            track.Distribute(request.DspIds, DateTime.UtcNow);
        }
        catch (DomainException ex)
        {
            return Result<TrackDetailResponse>.Conflict(ex.Message);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<Result<TrackDetailResponse>> UpdateStatusAsync(Guid id, UpdateTrackStatusRequest request, CancellationToken cancellationToken = default)
    {
        var track = await _dbContext.Tracks
            .Include(t => t.Distributions)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (track == null)
        {
            return Result<TrackDetailResponse>.NotFound($"Track with ID '{id}' was not found.");
        }

        var targetStatus = Enum.Parse<TrackStatus>(request.Status, true);

        try
        {
            track.ChangeStatus(targetStatus);
        }
        catch (DomainException ex)
        {
            return Result<TrackDetailResponse>.Conflict(ex.Message);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<Result<TrackDetailResponse>> UpdateDistributionStatusAsync(
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
            return Result<TrackDetailResponse>.NotFound($"Track with ID '{id}' was not found.");
        }

        var targetStatus = Enum.Parse<DistributionStatus>(request.Status, true);

        try
        {
            track.SetDistributionStatus(dspId, targetStatus);
        }
        catch (DomainException ex)
        {
            return Result<TrackDetailResponse>.Conflict(ex.Message);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }
}
