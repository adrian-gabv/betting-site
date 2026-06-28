using System.Security.Claims;

namespace BettingSite.API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUsername(this ClaimsPrincipal user) =>
            user.FindFirst(ClaimTypes.Name)?.Value
                ?? throw new InvalidOperationException("Name claim not present on authenticated user");
    }
}
