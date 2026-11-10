using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TakweneTask.Api.Extensions;
using TakweneTask.Application.DTOs.Auth;
using TakweneTask.Application.Services.Auth;

namespace TakweneTask.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Authenticates demo credentials and returns a signed JWT bearer token.
    /// Note: Uses a simplified configuration-based login strictly to fulfill task requirements (not production best practice).
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return result.ToActionResult(this);
    }
}
