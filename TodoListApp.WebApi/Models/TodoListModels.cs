using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApi.Models;

public class TodoListModel
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }
}

public class CreateTodoListModel
{
    [Required, MaxLength(100)]
    public string? Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }
}

public class UpdateTodoListModel
{
    [Required, MaxLength(100)]
    public string? Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }
}
