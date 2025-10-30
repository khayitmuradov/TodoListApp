using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApi.Models;

public class TaskModel
{
    public int Id { get; set; }

    public int TodoListId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? DueDate { get; set; }

    public Constraints.TaskStatus? Status { get; set; } = Constraints.TaskStatus.NotStarted;

    public string AssigneeId { get; set; } = default!;

    public bool IsOverdue { get; set; }

    public IReadOnlyList<TagModel> Tags { get; set; } = Array.Empty<TagModel>();
}

public class CreateTaskModel
{
    [Required, MaxLength(150)]
    public string Title { get; set; } = default!;

    [MaxLength(4000)]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    [Required]
    public Constraints.TaskStatus? Status { get; set; }
}

public class UpdateTaskModel
{
    [Required, MaxLength(150)]
    public string Title { get; set; } = default!;

    [MaxLength(4000)]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    [Required]
    public Constraints.TaskStatus? Status { get; set; } = default!;

    public string? AssigneeId { get; set; }
}
