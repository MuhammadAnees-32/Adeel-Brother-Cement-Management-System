using AdeelBrotherCement.Application;
using AdeelBrotherCement.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AdeelBrotherCement.Api.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireScreenAttribute : Attribute, IAuthorizationFilter
{
    private readonly AppScreen[] _screens;

    public RequireScreenAttribute(params AppScreen[] screens) => _screens = screens;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (user.IsInRole(UserRole.Admin.ToString()))
            return;

        var screensClaim = user.FindFirst("screens")?.Value;
        var allowedScreens = ScreenPermissions.FromClaimValue(screensClaim);

        if (_screens.Any(screen => allowedScreens.Contains(screen)))
            return;

        context.Result = new ForbidResult();
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireAdminAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!user.IsInRole(UserRole.Admin.ToString()))
            context.Result = new ForbidResult();
    }
}
