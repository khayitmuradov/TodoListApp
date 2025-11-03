using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoListApp.WebApi.Domain;

internal class TaskCommentEntity
{
    public int Id { get; set; }

    [Required]
    public int TaskId { get; set; }

    [ForeignKey(nameof(TaskId))]
    public TaskEntity Task { get; set; } = default!;

    [Required, MaxLength(2000)]
    public string Text { get; set; } = default!;

    [Required]
    public string CreatedByUserId { get; set; } = default!;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedUtc { get; set; }
}
