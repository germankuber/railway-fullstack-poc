using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Endpoints;
using TodoApi.Hubs;
using Serilog;
using TodoApi.Infrastructure;
using TodoApi.Infrastructure.Logging;

Log.Logger = LoggingConfiguration.CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddSerilog((_, logger) => logger.Configure(builder.Configuration));

    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(DatabaseUrlConverter.Resolve(builder.Configuration)));

    builder.Services.AddDataProtection()
        .SetApplicationName("todo-api")
        .PersistKeysToDbContext<AppDbContext>();

    builder.Services
        .AddIdentityApiEndpoints<IdentityUser>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
        })
        .AddEntityFrameworkStores<AppDbContext>();

    builder.Services.PostConfigure<BearerTokenOptions>(IdentityConstants.BearerScheme, options =>
    {
        options.Events.OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            if (!string.IsNullOrEmpty(accessToken) && context.Request.Path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        };
    });

    builder.Services.AddProblemDetails();
    builder.Services.AddAuthorization();
    builder.Services.AddSignalR();

    var allowedOrigins = (Environment.GetEnvironmentVariable("WEB_ORIGIN") ?? "http://localhost:5173")
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    builder.Services.AddCors(options =>
        options.AddDefaultPolicy(policy => policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()));

    var app = builder.Build();

    if (args.Contains(MigrateCommand.Argument))
    {
        await DatabaseMigrator.MigrateWithRetryAsync(app.Services);
        return 0;
    }

    StartupLog.ApiListening(app.Logger, port, allowedOrigins);

    app.UseApiRequestLogging();
    app.UseExceptionHandler();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
    app.MapGroup("/auth").MapIdentityApi<IdentityUser>();
    app.MapTodoEndpoints();
    app.MapHub<TodoHub>("/hubs/todos");

    await app.RunAsync();
}
catch (Exception exception) when (exception is not HostAbortedException)
{
    Log.Fatal(exception, "API terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

return 0;
