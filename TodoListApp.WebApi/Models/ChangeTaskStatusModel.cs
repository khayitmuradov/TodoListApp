using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApi.Models;

public class ChangeTaskStatusModel
{
    [Required]
    public Constraints.TaskStatus Status { get; set; }
}
