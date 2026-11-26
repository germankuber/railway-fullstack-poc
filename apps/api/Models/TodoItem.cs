namespace TodoApi.Models;

public class TodoItem
{
    public const int TitleMaxLength = 200;

    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsDone { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string OwnerId { get; set; } = string.Empty;
}
