namespace AdeelBrotherCement.Application.DTOs;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<string> AllowedScreens { get; set; } = [];
}

public class CurrentUserResponse
{
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<string> AllowedScreens { get; set; } = [];
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<string> AllowedScreens { get; set; } = [];
    public bool IsActive { get; set; }
}

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<string>? AllowedScreens { get; set; }
}

public class UpdateUserRequest
{
    public string? Password { get; set; }
    public string Role { get; set; } = string.Empty;
    public List<string> AllowedScreens { get; set; } = [];
    public bool IsActive { get; set; } = true;
}

public class ScreenInfoDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}
