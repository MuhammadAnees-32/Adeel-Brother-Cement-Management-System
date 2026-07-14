using AdeelBrotherCement.Domain.Enums;

namespace AdeelBrotherCement.Domain.Entities;

public class AppUser
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public List<AppScreen> AllowedScreens { get; set; } = [];
    public bool IsActive { get; set; } = true;
}
