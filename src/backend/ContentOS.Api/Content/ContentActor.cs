using System.Security.Claims;

namespace ContentOS.Api.Content;

internal static class ContentActor
{
    public static bool TryGetUserId(HttpContext httpContext, out Guid userId)
    {
        var value = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out userId);
    }
}
