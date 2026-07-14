using AdeelBrotherCement.Domain.Enums;

namespace AdeelBrotherCement.Application;

public static class ScreenPermissions
{
    public static readonly AppScreen[] AllScreens = Enum.GetValues<AppScreen>();

    public static readonly AppScreen[] SalesmanDefaults =
    [
        AppScreen.NewSale,
        AppScreen.CustomerBalance,
        AppScreen.Inventory,
        AppScreen.Expenses
    ];

    public static IReadOnlyList<AppScreen> GetEffectiveScreens(UserRole role, IEnumerable<AppScreen> allowedScreens)
    {
        if (role == UserRole.Admin)
            return AllScreens;

        return allowedScreens.Distinct().ToList();
    }

    public static bool HasAccess(UserRole role, IEnumerable<AppScreen> allowedScreens, AppScreen screen)
        => GetEffectiveScreens(role, allowedScreens).Contains(screen);

    public static string ToClaimValue(IEnumerable<AppScreen> screens)
        => string.Join(',', screens.Select(s => s.ToString()));

    public static List<AppScreen> FromClaimValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return [];

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => Enum.TryParse<AppScreen>(s, out var screen) ? screen : (AppScreen?)null)
            .Where(s => s.HasValue)
            .Select(s => s!.Value)
            .Distinct()
            .ToList();
    }
}
