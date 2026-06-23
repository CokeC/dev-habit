using System.Security.Claims;

namespace DevHabit.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string? GetIdentityId(this ClaimsPrincipal? principal)
    {
        var identityId = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        return identityId;
    }
}