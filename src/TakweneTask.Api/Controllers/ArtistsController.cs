using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<ArtistResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ArtistResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _artistService.GetAllAsync(cancellationToken);
        return Ok(result);
    }
}
