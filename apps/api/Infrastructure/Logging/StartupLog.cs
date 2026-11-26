namespace TodoApi.Infrastructure.Logging;

public static partial class StartupLog
{
    [LoggerMessage(Level = LogLevel.Information, Message = "API listening on port {Port} with allowed origins {AllowedOrigins}")]
    public static partial void ApiListening(ILogger logger, string port, string[] allowedOrigins);
}
