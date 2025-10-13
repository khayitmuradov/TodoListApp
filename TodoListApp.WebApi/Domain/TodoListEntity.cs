namespace TodoListApp.WebApi.Domain;

internal class TodoListEntity
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string OwnerId { get; set; } = default!;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedUtc { get; set; }

    public ICollection<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();
}
