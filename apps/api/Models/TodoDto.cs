namespace TodoApi.Models;

public record TodoDto(Guid Id, string Title, bool IsDone, DateTimeOffset CreatedAt)
{
    public static TodoDto From(TodoItem item) => new(item.Id, item.Title, item.IsDone, item.CreatedAt);
}

public record CreateTodoRequest(string? Title);

public record UpdateTodoRequest(string? Title, bool? IsDone);
