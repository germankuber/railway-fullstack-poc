using System.Security.Claims;
using Serilog;
using Serilog.Events;

namespace TodoApi.Infrastructure.Logging;

public static class RequestLoggingExtensions
{
    private const string HealthPath = "/health";

    public static IApplicationBuilder UseApiRequestLogging(this IApplicationBuilder app) =>
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath:l} responded {StatusCode} in {Elapsed:0.0} ms";
            options.GetLevel = ResolveLevel;
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("UserId", UserIdOf(httpContext));
                diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress?.ToString());
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
            };
        });

    private static LogEventLevel ResolveLevel(HttpContext httpContext, double elapsedMs, Exception? exception)
    {
        if (httpContext.Request.Path.StartsWithSegments(HealthPath) && exception is null && httpContext.Response.StatusCode < 500)
        {
            return LogEventLevel.Verbose;
        }

        return exception is not null || httpContext.Response.StatusCode >= 500
            ? LogEventLevel.Error
            : httpContext.Response.StatusCode >= 400
                ? LogEventLevel.Warning
                : LogEventLevel.Information;
    }

    private static string? UserIdOf(HttpContext httpContext) =>
        httpContext.User.Identity?.IsAuthenticated == true
            ? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;
}
