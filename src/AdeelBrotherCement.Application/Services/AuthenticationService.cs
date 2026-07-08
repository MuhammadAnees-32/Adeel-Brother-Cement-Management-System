using AdeelBrotherCement.Application.DTOs;

namespace AdeelBrotherCement.Application.Services;

public class AuthenticationService
{
    public LoginResponse? Login(LoginRequest request)
    {
        if (request.Username == "MuhammadAnees" &&
            request.Password == "MAnees@2026!")
        {
            return new LoginResponse
            {
                Username = "MuhammadAnees",
                Token = "temporary-token"
            };
        }

        return null;
    }
}