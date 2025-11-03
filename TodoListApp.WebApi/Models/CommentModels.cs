using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApi.Models;

public class CommentModel
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public string Text { get; set; } = default!;

    public string CreatedByUserId { get; set; } = default!;

    public DateTime CreatedUtc { get; set; }

    public DateTime? UpdatedUtc { get; set; }
}

public class CreateCommentModel
{
    [Required, MaxLength(2000)]
    public string Text { get; set; } = default!;
}

public class UpdateCommentModel
{
    [Required, MaxLength(2000)]
    public string Text { get; set; } = default!;
}
