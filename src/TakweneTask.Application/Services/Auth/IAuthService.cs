using TakweneTask.Application.DTOs.Auth;

namespace TakweneTask.Application.Services.Auth;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
