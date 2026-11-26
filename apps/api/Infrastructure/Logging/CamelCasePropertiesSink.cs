using System.Text;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;

namespace TodoApi.Infrastructure.Logging;

public sealed class CamelCasePropertiesSink(ILogEventSink inner) : ILogEventSink
{
    private static readonly IReadOnlyDictionary<string, string> Renames = new Dictionary<string, string>
    {
        ["Elapsed"] = "elapsedMs"
    };

    private static readonly HashSet<string> ReservedNames = new(StringComparer.Ordinal)
    {
        "timestamp", "level", "message", "exception"
    };

    public void Emit(LogEvent logEvent)
    {
        var renderedTemplate = new MessageTemplate([new TextToken(RenderMessage(logEvent))]);

        var properties = logEvent.Properties
            .Select(property => new LogEventProperty(TargetName(property.Key), property.Value))
            .DistinctBy(property => property.Name);

        inner.Emit(new LogEvent(
            logEvent.Timestamp,
            logEvent.Level,
            logEvent.Exception,
            renderedTemplate,
            properties,
            logEvent.TraceId ?? default,
            logEvent.SpanId ?? default));
    }

    private static string RenderMessage(LogEvent logEvent)
    {
        var builder = new StringBuilder();

        foreach (var token in logEvent.MessageTemplate.Tokens)
        {
            switch (token)
            {
                case PropertyToken property when logEvent.Properties.TryGetValue(property.PropertyName, out var value):
                    builder.Append(value is ScalarValue { Value: string text } ? text : value.ToString(property.Format, null));
                    break;
                case PropertyToken property:
                    builder.Append(property);
                    break;
                default:
                    builder.Append(token);
                    break;
            }
        }

        return builder.ToString();
    }

    private static string TargetName(string name)
    {
        var candidate = Renames.TryGetValue(name, out var renamed)
            ? renamed
            : char.ToLowerInvariant(name[0]) + name[1..];

        return ReservedNames.Contains(candidate) ? $"attr{char.ToUpperInvariant(candidate[0])}{candidate[1..]}" : candidate;
    }
}
