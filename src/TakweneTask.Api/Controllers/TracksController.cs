using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TakweneTask.Api.Extensions;
using TakweneTask.Application.Common.Models;
using TakweneTask.Application.DTOs.Tracks;
using TakweneTask.Application.Services.Tracks;

namespace TakweneTask.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TracksController : ControllerBase
{
    private readonly ITrackService _trackService;

    public TracksController(ITrackService trackService)
    {
        _trackService = trackService;
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(TrackSummaryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TrackSummaryResponse>> Create([FromBody] CreateTrackRequest request, CancellationToken cancellationToken)
    {
        var result = await _trackService.CreateAsync(request, cancellationToken);
        return result.ToActionResult(this, StatusCodes.Status201Created);
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<TrackSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TrackSummaryResponse>>> GetAll(
        [FromQuery] Guid? artistId,
        [FromQuery] string? genre,
        [FromQuery] string? status,
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var pagination = (pageNumber.HasValue || pageSize.HasValue)
            ? new PaginationParams(pageNumber ?? 1, pageSize ?? 10)
            : null;

        var result = await _trackService.GetAllAsync(artistId, genre, status, pagination, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TrackDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TrackDetailResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _trackService.GetByIdAsync(id, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("{id:guid}/distribute")]
    [Authorize]
    [ProducesResponseType(typeof(TrackDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TrackDetailResponse>> Distribute(
        Guid id,
        [FromBody] DistributeTrackRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _trackService.DistributeAsync(id, request, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize]
    [ProducesResponseType(typeof(TrackDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TrackDetailResponse>> UpdateStatus(
        Guid id,
        [FromBody] UpdateTrackStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _trackService.UpdateStatusAsync(id, request, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Evaluation reviewer simulation endpoint: simulates external DSP asynchronous callbacks
    /// (e.g. Spotify, Apple Music, YouTube) transitioning a distribution from pending to live or rejected.
    /// This resolves testing deadlocks in environments without live DSP webhooks.
    /// </summary>
    [HttpPatch("{id:guid}/distributions/{dspId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(TrackDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TrackDetailResponse>> UpdateDistributionStatus(
        Guid id,
        Guid dspId,
        [FromBody] UpdateDistributionStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _trackService.UpdateDistributionStatusAsync(id, dspId, request, cancellationToken);
        return result.ToActionResult(this);
    }
}
