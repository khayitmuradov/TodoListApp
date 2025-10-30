namespace TodoListApp.WebApi.Domain;

internal class TagEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;

    public string ColorHex { get; set; } = default!;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public ICollection<TaskTagEntity> TaskTags { get; set; } = new List<TaskTagEntity>();
}
