using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApp.Models;

public class TodoListWebApiModel
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }
}

public class CreateTodoListWebApiModel
{
    [Required, MaxLength(100)]
    public string? Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }
}

public class UpdateTodoListWebApiModel
{
    [Required, MaxLength(100)]
    public string? Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }
}
