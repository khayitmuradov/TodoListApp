namespace TodoListApp.WebApi.Services.Models;

public class TodoList
{
    public int Id { get; init; }

    public string? Title { get; init; }

    public string? Description { get; init; }

    public string OwnerId { get; init; } = default!;
}
