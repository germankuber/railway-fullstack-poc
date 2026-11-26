using Serilog.Core;
using Serilog.Events;

namespace TodoApi.Infrastructure.Logging;

public static class ServiceMetadata
{
    public static string EnvironmentName { get; } =
        FirstNonEmpty("RAILWAY_ENVIRONMENT_NAME", "ASPNETCORE_ENVIRONMENT") ?? "local";

    public static string? DeploymentId { get; } = FirstNonEmpty("RAILWAY_DEPLOYMENT_ID");

    private static string? FirstNonEmpty(params string[] variableNames) =>
        variableNames
            .Select(Environment.GetEnvironmentVariable)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
}

public sealed class DeploymentIdEnricher(string deploymentId) : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory) =>
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("deploymentId", deploymentId));
}

public sealed class NullEnricher : ILogEventEnricher
{
    public static readonly NullEnricher Instance = new();

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
    }
}
