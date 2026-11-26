using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Hubs;
using TodoApi.Models;

namespace TodoApi.Endpoints;

public sealed class TodoEndpointsLogCategory;

public static partial class TodoEndpoints
{
    public static IEndpointRouteBuilder MapTodoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/todos").RequireAuthorization();

        group.MapGet("/", ListTodos);
        group.MapPost("/", CreateTodo);
        group.MapPatch("/{id:guid}", UpdateTodo);
        group.MapDelete("/{id:guid}", DeleteTodo);

        return app;
    }

    private static string CurrentUserId(ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Authenticated user has no identifier claim.");

    private static bool IsValidTitle(string? title) =>
        !string.IsNullOrWhiteSpace(title) && title.Trim().Length <= TodoItem.TitleMaxLength;

    private static async Task<IResult> ListTodos(ClaimsPrincipal user, AppDbContext db)
    {
        var userId = CurrentUserId(user);

        var todos = await db.Todos
            .Where(todo => todo.OwnerId == userId)
            .OrderBy(todo => todo.CreatedAt)
            .ToListAsync();

        return Results.Ok(todos.Select(TodoDto.From));
    }

    private static async Task<IResult> CreateTodo(
        CreateTodoRequest request,
        ClaimsPrincipal user,
        AppDbContext db,
        IHubContext<TodoHub> hub,
        ILogger<TodoEndpointsLogCategory> logger)
    {
        if (!IsValidTitle(request.Title))
        {
            return Results.BadRequest(new { error = $"Title is required and must be at most {TodoItem.TitleMaxLength} characters." });
        }

        var userId = CurrentUserId(user);

        var todo = new TodoItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title!.Trim(),
            IsDone = false,
            CreatedAt = DateTimeOffset.UtcNow,
            OwnerId = userId
        };

        db.Todos.Add(todo);
        await db.SaveChangesAsync();

        var dto = TodoDto.From(todo);
        LogTodoCreated(logger, todo.Id, userId);
        await hub.Clients.User(userId).SendAsync("TodoCreated", dto);

        return Results.Created($"/api/todos/{todo.Id}", dto);
    }

    private static async Task<IResult> UpdateTodo(
        Guid id,
        UpdateTodoRequest request,
        ClaimsPrincipal user,
        AppDbContext db,
        IHubContext<TodoHub> hub,
        ILogger<TodoEndpointsLogCategory> logger)
    {
        var userId = CurrentUserId(user);
        var todo = await db.Todos.FirstOrDefaultAsync(item => item.Id == id && item.OwnerId == userId);

        if (todo is null)
        {
            LogTodoNotFound(logger, id, userId);
            return Results.NotFound();
        }

        if (request.Title is not null)
        {
            if (!IsValidTitle(request.Title))
            {
                return Results.BadRequest(new { error = $"Title must be non-empty and at most {TodoItem.TitleMaxLength} characters." });
            }

            todo.Title = request.Title.Trim();
        }

        if (request.IsDone is not null)
        {
            todo.IsDone = request.IsDone.Value;
        }

        await db.SaveChangesAsync();

        var dto = TodoDto.From(todo);
        LogTodoUpdated(logger, todo.Id, userId);
        await hub.Clients.User(userId).SendAsync("TodoUpdated", dto);

        return Results.Ok(dto);
    }

    private static async Task<IResult> DeleteTodo(
        Guid id,
        ClaimsPrincipal user,
        AppDbContext db,
        IHubContext<TodoHub> hub,
        ILogger<TodoEndpointsLogCategory> logger)
    {
        var userId = CurrentUserId(user);
        var todo = await db.Todos.FirstOrDefaultAsync(item => item.Id == id && item.OwnerId == userId);

        if (todo is null)
        {
            LogTodoNotFound(logger, id, userId);
            return Results.NotFound();
        }

        db.Todos.Remove(todo);
        await db.SaveChangesAsync();

        LogTodoDeleted(logger, id, userId);
        await hub.Clients.User(userId).SendAsync("TodoDeleted", id);

        return Results.NoContent();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Todo {TodoId} created by user {UserId}")]
    private static partial void LogTodoCreated(ILogger logger, Guid todoId, string userId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Todo {TodoId} updated by user {UserId}")]
    private static partial void LogTodoUpdated(ILogger logger, Guid todoId, string userId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Todo {TodoId} deleted by user {UserId}")]
    private static partial void LogTodoDeleted(ILogger logger, Guid todoId, string userId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Todo {TodoId} not found for user {UserId}")]
    private static partial void LogTodoNotFound(ILogger logger, Guid todoId, string userId);
}
