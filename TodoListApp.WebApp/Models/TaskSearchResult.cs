namespace TodoListApp.WebApp.Models;

public class TaskSearchResult
{
    public IReadOnlyList<TaskItem>? Items { get; init; }

    public int Total { get; init; }

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)this.Total / Math.Max(1, this.PageSize)));

    public TaskSearchQuery Query { get; init; } = new TaskSearchQuery();
}
