using AutoMapper;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TrackService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<TrackSummaryResponse>> CreateAsync(CreateTrackRequest request, CancellationToken cancellationToken = default)
    {
        var artist = await _unitOfWork.Artists.GetByIdAsync(request.ArtistId, cancellationToken);
        if (artist == null)
        {
            return Result<TrackSummaryResponse>.NotFound($"Artist with ID '{request.ArtistId}' was not found.");
        }

        var track = new Track(
            Guid.CreateVersion7(),
            request.Title.Trim(),
            artist.Id,
            Isrc.Normalize(request.Isrc),
            request.ReleaseDate,
            request.Genre.Trim()
        );

        await _unitOfWork.Tracks.AddAsync(track, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = _mapper.Map<TrackSummaryResponse>(track);
        response.ArtistName = artist.Name;

        return Result<TrackSummaryResponse>.Success(response);
    }

    public async Task<Result<IReadOnlyList<TrackSummaryResponse>>> GetAllAsync(
        Guid? artistId,
        string? genre,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var trackStatus = ParseTrackStatus(status);
        var tracks = await _unitOfWork.Tracks.GetAllAsync(artistId, genre, trackStatus, cancellationToken);

        var response = _mapper.Map<IReadOnlyList<TrackSummaryResponse>>(tracks);
        return Result<IReadOnlyList<TrackSummaryResponse>>.Success(response);
    }

    public async Task<Result<PagedResult<TrackSummaryResponse>>> GetPagedAsync(
        Guid? artistId,
        string? genre,
        string? status,
        PaginationParams pagination,
        CancellationToken cancellationToken = default)
    {
        var trackStatus = ParseTrackStatus(status);
        var pagedTracks = await _unitOfWork.Tracks.GetPagedAsync(artistId, genre, trackStatus, pagination, cancellationToken);

        var mappedItems = _mapper.Map<IReadOnlyList<TrackSummaryResponse>>(pagedTracks.Items);
        var response = new PagedResult<TrackSummaryResponse>(
            mappedItems,
            pagedTracks.TotalCount,
            pagedTracks.PageNumber,
            pagedTracks.PageSize);

        return Result<PagedResult<TrackSummaryResponse>>.Success(response);
    }

    public async Task<Result<TrackDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var track = await _unitOfWork.Tracks.GetByIdWithDetailsAsync(id, cancellationToken);

        if (track == null)
        {
            return Result<TrackDetailResponse>.NotFound($"Track with ID '{id}' was not found.");
        }

        var response = _mapper.Map<TrackDetailResponse>(track);
        return Result<TrackDetailResponse>.Success(response);
    }

    public async Task<Result<TrackDetailResponse>> DistributeAsync(Guid id, DistributeTrackRequest request, CancellationToken cancellationToken = default)
    {
        var track = await _unitOfWork.Tracks.GetByIdWithDistributionsAsync(id, cancellationToken);

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

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<Result<TrackDetailResponse>> UpdateStatusAsync(Guid id, UpdateTrackStatusRequest request, CancellationToken cancellationToken = default)
    {
        var track = await _unitOfWork.Tracks.GetByIdWithDistributionsAsync(id, cancellationToken);

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

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<Result<TrackDetailResponse>> UpdateDistributionStatusAsync(
        Guid id,
        Guid dspId,
        UpdateDistributionStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var track = await _unitOfWork.Tracks.GetByIdWithDistributionsAsync(id, cancellationToken);

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

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    private static TrackStatus? ParseTrackStatus(string? status)
    {
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TrackStatus>(status, true, out var trackStatus))
        {
            return trackStatus;
        }

        return null;
    }
}
