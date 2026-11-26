using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TodoApi.Hubs;

[Authorize]
public partial class TodoHub(ILogger<TodoHub> logger) : Hub
{
    public override async Task OnConnectedAsync()
    {
        LogClientConnected(Context.UserIdentifier, Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception is null)
        {
            LogClientDisconnected(Context.UserIdentifier, Context.ConnectionId);
        }
        else
        {
            LogClientDisconnectedWithError(exception, Context.UserIdentifier, Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "SignalR client {ConnectionId} connected for user {UserId}")]
    private partial void LogClientConnected(string? userId, string connectionId);

    [LoggerMessage(Level = LogLevel.Information, Message = "SignalR client {ConnectionId} disconnected for user {UserId}")]
    private partial void LogClientDisconnected(string? userId, string connectionId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "SignalR client {ConnectionId} disconnected with error for user {UserId}")]
    private partial void LogClientDisconnectedWithError(Exception exception, string? userId, string connectionId);
}
