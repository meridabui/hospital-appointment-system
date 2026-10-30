using HospitalAppointment.Api.Data.DTOs.Auth;
using HospitalAppointment.Api.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HospitalAppointment.Api.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IConfiguration _configuration;

    public AuthService(
        IAuthRepository authRepository,
        IConfiguration configuration)
    {
        _authRepository = authRepository;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var user = await _authRepository.GetUserByUsernameOrEmailAsync(
            request.UsernameOrEmail.Trim(),
            cancellationToken);

        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "Invalid username or password.");
        }

        // Verify BCrypt password
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash);

        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException(
                "Invalid username or password.");
        }

        var jwtSection = _configuration.GetSection("Jwt");

        var key = jwtSection["Key"]
            ?? throw new InvalidOperationException("JWT Key is missing.");

        var issuer = jwtSection["Issuer"]
            ?? throw new InvalidOperationException("JWT Issuer is missing.");

        var audience = jwtSection["Audience"]
            ?? throw new InvalidOperationException("JWT Audience is missing.");

        var expirationMinutes = int.TryParse(
            jwtSection["ExpirationInMinutes"], out var minutes)
            ? minutes : 60;

        if (expirationMinutes <= 0)
            throw new InvalidOperationException("Invalid JWT expiration.");

        var keyBytes = Encoding.UTF8.GetBytes(key);

        if (keyBytes.Length < 32)
            throw new InvalidOperationException("JWT Key must be at least 32 bytes.");

        var securityKey = new SymmetricSecurityKey(keyBytes);

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.RoleName)
        };

        var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new LoginResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt,
            UserId = user.UserId,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.RoleName
        };
    }
}