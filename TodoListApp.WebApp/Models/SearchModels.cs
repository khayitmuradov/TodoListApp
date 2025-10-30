using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApp.Models;

public class TaskSearchQuery
{
    public string? Title { get; set; }

    [DataType(DataType.Date)]
    public DateTime? CreatedFrom { get; set; }

    [DataType(DataType.Date)]
    public DateTime? CreatedTo { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DueFrom { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DueTo { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public bool IsTitleMode => !string.IsNullOrWhiteSpace(this.Title);

    public bool IsCreatedMode => this.CreatedFrom.HasValue || this.CreatedTo.HasValue;

    public bool IsDueMode => this.DueFrom.HasValue || this.DueTo.HasValue;
}
