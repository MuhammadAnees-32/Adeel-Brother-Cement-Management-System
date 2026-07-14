using AdeelBrotherCement.Application.DTOs;
using AdeelBrotherCement.Application.Interfaces;
using AdeelBrotherCement.Domain.Entities;
using AdeelBrotherCement.Domain.Enums;

namespace AdeelBrotherCement.Application.Services;

public class UserService(IUserRepository userRepository)
{
    private static readonly Dictionary<AppScreen, string> ScreenLabels = new()
    {
        [AppScreen.Dashboard] = "Dashboard",
        [AppScreen.NewSale] = "New Sale",
        [AppScreen.SalesHistory] = "Sales History",
        [AppScreen.CustomerBalance] = "Customer Balance",
        [AppScreen.Inventory] = "Inventory",
        [AppScreen.Expenses] = "Expenses",
        [AppScreen.UserManagement] = "User Management"
    };

    public IReadOnlyList<ScreenInfoDto> GetAvailableScreens()
        => ScreenPermissions.AllScreens
            .Select(s => new ScreenInfoDto { Key = s.ToString(), Label = ScreenLabels[s] })
            .ToList();

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken ct = default)
    {
        var users = await userRepository.GetAllAsync(ct);
        return users.Select(ToDto).ToList();
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(id, ct);
        return user is null ? null : ToDto(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            throw new ArgumentException("Username is required.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Password is required.");

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            throw new ArgumentException("Invalid role. Use Admin or Salesman.");

        var existing = await userRepository.GetByUsernameAsync(request.Username.Trim(), ct);
        if (existing is not null)
            throw new InvalidOperationException("Username already exists.");

        var allowedScreens = ParseScreens(request.AllowedScreens, role);

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Username = request.Username.Trim(),
            PasswordHash = PasswordHasher.Hash(request.Password),
            Role = role,
            AllowedScreens = allowedScreens,
            IsActive = true
        };

        var created = await userRepository.CreateAsync(user, ct);
        return ToDto(created);
    }

    public async Task<UserDto?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(id, ct);
        if (user is null) return null;

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            throw new ArgumentException("Invalid role. Use Admin or Salesman.");

        user.Role = role;
        user.AllowedScreens = ParseScreens(request.AllowedScreens, role);
        user.IsActive = request.IsActive;

        if (!string.IsNullOrWhiteSpace(request.Password))
            user.PasswordHash = PasswordHasher.Hash(request.Password);

        var updated = await userRepository.UpdateAsync(user, ct);
        return ToDto(updated);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        => await userRepository.DeleteAsync(id, ct);

    private static List<AppScreen> ParseScreens(IEnumerable<string>? screens, UserRole role)
    {
        if (role == UserRole.Admin)
            return ScreenPermissions.AllScreens.ToList();

        if (screens is null || !screens.Any())
            return ScreenPermissions.SalesmanDefaults.ToList();

        return screens
            .Select(s => Enum.TryParse<AppScreen>(s, true, out var screen) ? screen : (AppScreen?)null)
            .Where(s => s.HasValue)
            .Select(s => s!.Value)
            .Distinct()
            .ToList();
    }

    private static UserDto ToDto(AppUser user)
    {
        var effectiveScreens = ScreenPermissions.GetEffectiveScreens(user.Role, user.AllowedScreens);
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role.ToString(),
            AllowedScreens = effectiveScreens.Select(s => s.ToString()).ToList(),
            IsActive = user.IsActive
        };
    }
}
