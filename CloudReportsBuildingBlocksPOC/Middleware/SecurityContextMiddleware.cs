namespace CloudReportsBuildingBlocksPOC.Middleware;

using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using CloudReportsBuildingBlocksPOC.Services.Abstractions;

public class SecurityContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityContextMiddleware> _logger;

    public SecurityContextMiddleware(
        RequestDelegate next,
        ILogger<SecurityContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUserContextService userContextService)
    {
        try
        {
            // Extract information from authenticated user.
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? context.User.FindFirst("sub")?.Value;

                // Set the user context for the request.
                userContextService.Context = new () { UserId = userId };

                _logger.LogDebug("Security context established for user: {UserId}", userId);
            }
            else
            {
                _logger.LogDebug("Request without authenticated user");
            }

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SecurityContextMiddleware");
            throw;
        }
    }
}

public static class SecurityContextMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityContext(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityContextMiddleware>();
    }
}
