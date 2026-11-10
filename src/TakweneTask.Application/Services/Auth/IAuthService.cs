using TakweneTask.Application.Common.Models;
using TakweneTask.Application.DTOs.Auth;

namespace TakweneTask.Application.Services.Auth;

public interface IAuthService
{
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
