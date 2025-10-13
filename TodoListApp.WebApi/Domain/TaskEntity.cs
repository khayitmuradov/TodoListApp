using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoListApp.WebApi.Domain;

internal class TaskEntity
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string? Title { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? DueDate { get; set; }

    [Required]
    public Constraints.TaskStatus? Status { get; set; } = Constraints.TaskStatus.NotStarted;

    [Required]
    public string AssigneeId { get; set; } = default!;

    public int TodoListId { get; set; }

    [ForeignKey(nameof(TodoListId))]
    public TodoListEntity? TodoList { get; set; }
}
