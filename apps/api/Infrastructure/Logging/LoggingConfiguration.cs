using Serilog;
using Serilog.Events;
using Serilog.Templates;

namespace TodoApi.Infrastructure.Logging;

public static class LoggingConfiguration
{
    private const string JsonTemplate =
        "{ {" +
        "timestamp: ToString(UtcDateTime(@t), 'yyyy-MM-ddTHH:mm:ss.fffZ'), " +
        "level: if @l = 'Warning' then 'warn' " +
        "else if @l = 'Error' or @l = 'Fatal' then 'error' " +
        "else if @l = 'Information' then 'info' " +
        "else 'debug', " +
        "message: @m, " +
        "exception: @x, " +
        "..rest()" +
        "} }\n";

    public static Serilog.ILogger CreateBootstrapLogger() =>
        new LoggerConfiguration()
            .MinimumLevel.Information()
            .ApplyCommonSettings()
            .CreateBootstrapLogger();

    public static LoggerConfiguration Configure(this LoggerConfiguration logger, IConfiguration configuration)
    {
        logger.ReadFrom.Configuration(configuration).ApplyCommonSettings();

        if (Enum.TryParse<LogEventLevel>(configuration["LOG_LEVEL"], ignoreCase: true, out var level))
        {
            logger.MinimumLevel.Is(level);
        }

        return logger;
    }

    private static LoggerConfiguration ApplyCommonSettings(this LoggerConfiguration logger) =>
        logger
            .Enrich.FromLogContext()
            .Enrich.WithProperty("service", "api")
            .Enrich.WithProperty("environment", ServiceMetadata.EnvironmentName)
            .Enrich.With(ServiceMetadata.DeploymentId is { } deploymentId
                ? new DeploymentIdEnricher(deploymentId)
                : NullEnricher.Instance)
            .WriteTo.Sink(new CamelCasePropertiesSink(
                new LoggerConfiguration()
                    .WriteTo.Console(new ExpressionTemplate(JsonTemplate))
                    .CreateLogger()));
}
