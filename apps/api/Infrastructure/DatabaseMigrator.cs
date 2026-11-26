using Microsoft.EntityFrameworkCore;
using TodoApi.Data;

namespace TodoApi.Infrastructure;

public static partial class DatabaseMigrator
{
    private const int MaxAttempts = 10;
    private static readonly TimeSpan DelayBetweenAttempts = TimeSpan.FromSeconds(3);

    public static async Task MigrateWithRetryAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DatabaseMigrator).FullName!);

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                LogMigrationStarted(logger, attempt, MaxAttempts);

                using var scope = services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var pendingCount = (await db.Database.GetPendingMigrationsAsync()).Count();
                await db.Database.MigrateAsync();

                LogMigrationApplied(logger, pendingCount, attempt, MaxAttempts);
                return;
            }
            catch (Exception exception) when (attempt < MaxAttempts)
            {
                LogMigrationAttemptFailed(logger, exception, attempt, MaxAttempts, (int)DelayBetweenAttempts.TotalMilliseconds);
                await Task.Delay(DelayBetweenAttempts);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Applying database migrations (attempt {Attempt}/{MaxAttempts})")]
    private static partial void LogMigrationStarted(ILogger logger, int attempt, int maxAttempts);

    [LoggerMessage(Level = LogLevel.Information, Message = "Applied {AppliedCount} database migrations (attempt {Attempt}/{MaxAttempts})")]
    private static partial void LogMigrationApplied(ILogger logger, int appliedCount, int attempt, int maxAttempts);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Database migration attempt {Attempt}/{MaxAttempts} failed, retrying in {RetryDelayMs} ms")]
    private static partial void LogMigrationAttemptFailed(ILogger logger, Exception exception, int attempt, int maxAttempts, int retryDelayMs);
}
