namespace TodoListApp.WebApp.Models;

public enum TaskStatus
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2,
}

public class TaskItem
{
    public int Id { get; set; }

    public int TodoListId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public TaskStatus? Status { get; set; }

    public string? AssigneeId { get; set; }

    public bool? IsOverdue { get; set; }
}

public class TaskDetails : TaskItem
{
    public DateTime CreatedDate { get; set; }
}

public class CreateTaskRequest
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public TaskStatus Status { get; set; } = TaskStatus.NotStarted;
}

public class UpdateTaskRequest
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public TaskStatus? Status { get; set; }

    public string? AssigneeId { get; set; }
}

public class PatchTaskStatusRequest
{
    public TaskStatus Status { get; set; }
}
