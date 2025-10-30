namespace TodoListApp.WebApi.Domain;

internal class TaskTagEntity
{
    public int TaskId { get; set; }

    public TaskEntity Task { get; set; } = default!;

    public int TagId { get; set; }

    public TagEntity Tag { get; set; } = default!;

    public DateTime LinkedUtc { get; set; } = DateTime.UtcNow;
}
