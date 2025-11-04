namespace TodoListApp.WebApp.Models;

public class CommentItem
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public string Text { get; set; } = string.Empty;

    public string CreatedByUserId { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; }

    public DateTime? UpdatedUtc { get; set; }
}

public class CreateCommentRequest
{
    public string Text { get; set; } = string.Empty;
}

public class UpdateCommentRequest
{
    public string Text { get; set; } = string.Empty;
}
