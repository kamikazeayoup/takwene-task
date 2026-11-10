using TakweneTask.Application.Common.Models;
using TakweneTask.Application.DTOs.Auth;

namespace TakweneTask.Application.Services.Auth;

public interface IAuthService
{
    /// <summary>
    /// Authenticates demo credentials and generates a signed JWT token.
    /// </summary>
    /// <remarks>
    /// NOTE: Uses a simplified, configuration-based single-user login strictly
    /// to satisfy evaluation task requirements with zero setup overhead (not production best practice).
    /// </remarks>
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
