using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TakweneTask.Api.Extensions;
using TakweneTask.Application.Common.Models;
using TakweneTask.Application.DTOs.Artists;
using TakweneTask.Application.Services.Artists;

namespace TakweneTask.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArtistsController : ControllerBase
{
    private readonly IArtistService _artistService;

    public ArtistsController(IArtistService artistService)
    {
        _artistService = artistService;
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ArtistResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ArtistResponse>> Create([FromBody] CreateArtistRequest request, CancellationToken cancellationToken)
    {
        var result = await _artistService.CreateAsync(request, cancellationToken);
        return result.ToActionResult(this, StatusCodes.Status201Created);
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<ArtistResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ArtistResponse>>> GetAll(
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var pagination = (pageNumber.HasValue || pageSize.HasValue)
            ? new PaginationParams(pageNumber ?? 1, pageSize ?? 10)
            : null;

        var result = await _artistService.GetAllAsync(pagination, cancellationToken);
        return result.ToActionResult(this);
    }
}
