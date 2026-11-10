using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TakweneTask.Application.DTOs.Auth;

namespace TakweneTask.Application.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;

    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var configuredUsername = _configuration["DemoUser:Username"] ?? "demo";
        var configuredPassword = _configuration["DemoUser:Password"] ?? "demo-secret-key-12345";

        var usernameBytes = Encoding.UTF8.GetBytes(request.Username ?? string.Empty);
        var expectedUsernameBytes = Encoding.UTF8.GetBytes(configuredUsername);

        var passwordBytes = Encoding.UTF8.GetBytes(request.Password ?? string.Empty);
        var expectedPasswordBytes = Encoding.UTF8.GetBytes(configuredPassword);

        var isUsernameValid = usernameBytes.Length == expectedUsernameBytes.Length &&
                              CryptographicOperations.FixedTimeEquals(usernameBytes, expectedUsernameBytes);

        var isPasswordValid = passwordBytes.Length == expectedPasswordBytes.Length &&
                              CryptographicOperations.FixedTimeEquals(passwordBytes, expectedPasswordBytes);

        if (!isUsernameValid || !isPasswordValid)
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        var keyString = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT signing key is not configured.");

        var keyBytes = Encoding.UTF8.GetBytes(keyString);
        if (keyBytes.Length < 32)
        {
            throw new InvalidOperationException("JWT signing key must be at least 32 bytes (256 bits).");
        }

        var issuer = _configuration["Jwt:Issuer"] ?? "TakweneTask";
        var audience = _configuration["Jwt:Audience"] ?? "TakweneTask";
        var expiresAt = DateTime.UtcNow.AddMinutes(60);

        var securityKey = new SymmetricSecurityKey(keyBytes);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = configuredUsername,
            [JwtRegisteredClaimNames.Name] = configuredUsername,
            [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString()
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = audience,
            Claims = claims,
            Expires = expiresAt,
            SigningCredentials = credentials
        };

        var handler = new JsonWebTokenHandler();
        var token = handler.CreateToken(descriptor);

        return Task.FromResult(new LoginResponse
        {
            AccessToken = token,
            ExpiresAt = expiresAt
        });
    }
}
