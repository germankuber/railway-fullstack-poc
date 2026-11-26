using Npgsql;

namespace TodoApi.Infrastructure;

public static class DatabaseUrlConverter
{
    public static string ToNpgsqlConnectionString(string databaseUrl)
    {
        var uri = new Uri(databaseUrl);
        var credentials = uri.UserInfo.Split(':', 2);

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            GssEncryptionMode = GssEncryptionMode.Disable,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = Uri.UnescapeDataString(credentials[0]),
            Password = credentials.Length > 1 ? Uri.UnescapeDataString(credentials[1]) : null
        };

        return builder.ConnectionString;
    }

    public static string Resolve(IConfiguration configuration)
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            return ToNpgsqlConnectionString(databaseUrl);
        }

        return configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Set DATABASE_URL or ConnectionStrings:Default.");
    }
}
