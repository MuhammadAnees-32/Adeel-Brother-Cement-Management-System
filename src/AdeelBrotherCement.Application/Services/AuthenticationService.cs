using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AdeelBrotherCement.Application.DTOs;
using AdeelBrotherCement.Application.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AdeelBrotherCement.Application.Services;

public class JwtSettings
{
    public const string SectionName = "Jwt";
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "AdeelBrotherCement";
    public string Audience { get; set; } = "AdeelBrotherCementClient";
    public int ExpiryHours { get; set; } = 12;
}

public class AuthenticationService(IUserRepository userRepository, IOptions<JwtSettings> jwtOptions)
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return null;

        var user = await userRepository.GetByUsernameAsync(request.Username.Trim(), ct);
        if (user is null || !user.IsActive)
            return null;

        if (!PasswordHasher.Verify(request.Password, user.PasswordHash))
            return null;

        var effectiveScreens = ScreenPermissions.GetEffectiveScreens(user.Role, user.AllowedScreens);
        var token = GenerateToken(user, effectiveScreens);

        return new LoginResponse
        {
            Token = token,
            Username = user.Username,
            Role = user.Role.ToString(),
            AllowedScreens = effectiveScreens.Select(s => s.ToString()).ToList()
        };
    }

    private string GenerateToken(Domain.Entities.AppUser user, IReadOnlyList<Domain.Enums.AppScreen> screens)
    {
        var settings = jwtOptions.Value;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("screens", ScreenPermissions.ToClaimValue(screens))
        };

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(settings.ExpiryHours),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
