using AdeelBrotherCement.Application;
using AdeelBrotherCement.Application.DTOs;
using AdeelBrotherCement.Application.Services;
using AdeelBrotherCement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdeelBrotherCement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AuthenticationService authService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await authService.LoginAsync(request, ct);
        return result is null ? Unauthorized(new { message = "Invalid username or password." }) : Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<CurrentUserResponse> Me()
    {
        var username = User.FindFirstValue(ClaimTypes.Name);
        var role = User.FindFirstValue(ClaimTypes.Role);
        var screens = ScreenPermissions.FromClaimValue(User.FindFirstValue("screens"));

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(role))
            return Unauthorized();

        if (Enum.TryParse<UserRole>(role, out var parsedRole) && parsedRole == UserRole.Admin)
            screens = ScreenPermissions.AllScreens.ToList();

        return Ok(new CurrentUserResponse
        {
            Username = username,
            Role = role,
            AllowedScreens = screens.Select(s => s.ToString()).ToList()
        });
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController(UserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(CancellationToken ct)
        => Ok(await userService.GetAllAsync(ct));

    [HttpGet("screens")]
    public ActionResult<IReadOnlyList<ScreenInfoDto>> GetScreens()
        => Ok(userService.GetAvailableScreens());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken ct)
    {
        var user = await userService.GetByIdAsync(id, ct);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        try
        {
            var user = await userService.CreateAsync(request, ct);
            return Created(string.Empty, user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserDto>> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        try
        {
            var user = await userService.UpdateAsync(id, request, ct);
            return user is null ? NotFound() : Ok(user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => await userService.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
