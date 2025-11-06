using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApi.Exceptions;
using TodoListApp.WebApi.Models;
using TodoListApp.WebApi.Services;

namespace TodoListApp.WebApi.Controller;

[ApiController]
[Route("api")]
public partial class TaskController : ControllerBase
{
    private readonly ITaskDatabaseService service;
    private readonly IConfiguration configuration;

    public TaskController(
        ITaskDatabaseService service,
        IConfiguration configuration)
    {
        this.service = service;
        this.configuration = configuration;
    }

    [HttpGet("lists/{listId:int}/tasks")]
    public async Task<ActionResult<IEnumerable<TaskModel>>> GetByList(int listId, int page = 1, int pageSize = 0)
    {
        var (p, s) = this.ResolvePaging(page, pageSize);
        try
        {
            var (items, total) = await this.service.GetByListAsync(listId, this.CurrentUserId(), p, s);
            this.Response.Headers["X-Total-Count"] = total.ToString(CultureInfo.InvariantCulture);
            return this.Ok(items);
        }
        catch (KeyNotFoundException)
        {
            return this.NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return this.Forbid();
        }
    }

    [HttpGet("tasks/{id:int}")]
    public async Task<ActionResult<TaskModel>> GetById(int id)
    {
        try
        {
            return this.Ok(await this.service.GetByIdAsync(id, this.CurrentUserId()));
        }
        catch (KeyNotFoundException)
        {
            return this.NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return this.Forbid();
        }
    }

    [HttpPost("lists/{listId:int}/tasks")]
    public async Task<ActionResult<TaskModel>> Create(int listId, [FromBody] CreateTaskModel model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        try
        {
            var created = await this.service.CreateAsync(listId, this.CurrentUserId(), model);
            return this.CreatedAtAction(nameof(this.GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return this.BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return this.NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return this.Forbid();
        }
    }

    [HttpPut("tasks/{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskModel model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        try
        {
            await this.service.UpdateAsync(id, this.CurrentUserId(), model);
            return this.NoContent();
        }
        catch (ArgumentException ex)
        {
            return this.BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return this.NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return this.Forbid();
        }
    }

    [HttpDelete("tasks/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await this.service.DeleteAsync(id, this.CurrentUserId());
            return this.NoContent();
        }
        catch (KeyNotFoundException)
        {
            return this.NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return this.Forbid();
        }
    }

    /// <summary>
    /// Get tasks assigned to the current user with filter/sort/paging.
    /// </summary>
    /// <param name="status">NotStarted | InProgress | Completed (default when omitted: InProgress).</param>
    /// <param name="sortBy">name | dueDate (default: dueDate).</param>
    /// <param name="order">asc | desc (default: asc).</param>
    /// <param name="page">page number (default: 1).</param>
    /// <param name="pageSize">page size (uses config default/cap when 0).</param>
    [HttpGet("tasks/assigned-to-me")]
    public async Task<ActionResult<IEnumerable<TaskModel>>> GetAssignedToMeAsync(
        [FromQuery] string? status = "InProgress",
        [FromQuery] string? sortBy = "dueDate",
        [FromQuery] string? order = "asc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 0)
    {
        var (p, s) = this.ResolvePaging(page, pageSize);

        if (!string.IsNullOrWhiteSpace(status) &&
        !Enum.TryParse<Constraints.TaskStatus>(status, true, out _))
        {
            return this.BadRequest($"Invalid status '{status}'. Allowed: NotStarted, InProgress, Completed.");
        }

        try
        {
            var (items, total) = await this.service.GetAssignedToMeAsync(
                this.CurrentUserId(),
                status,
                sortBy ?? "dueDate",
                order ?? "asc",
                p,
                s);

            this.Response.Headers["X-Total-Count"] = total.ToString(CultureInfo.InvariantCulture);
            return this.Ok(items);
        }
        catch (AppException)
        {
            return this.StatusCode(500, "Internal Server Error");
        }
    }

    /// <summary>
    /// Change the status of a task. Allowed for assignee or list owner.
    /// </summary>
    [HttpPatch("tasks/{id:int}/status")]
    public async Task<IActionResult> ChangeStatusAsync(int id, [FromBody] ChangeTaskStatusModel model)
    {
        if (model is null)
        {
            return this.BadRequest("Request body cannot be null.");
        }

        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        try
        {
            await this.service.ChangeStatusAsync(id, this.CurrentUserId(), model.Status);
            return this.NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return this.Forbid();
        }
        catch (KeyNotFoundException)
        {
            return this.NotFound();
        }
    }

    [HttpGet("tasks/search")]
    public async Task<ActionResult<IEnumerable<TaskModel>>> Search(
    string? title,
    string? createdFrom,
    string? createdTo,
    string? dueFrom,
    string? dueTo,
    int page = 1,
    int pageSize = 0)
    {
        bool hasTitle = !string.IsNullOrWhiteSpace(title);
        bool hasCreated = !string.IsNullOrWhiteSpace(createdFrom) || !string.IsNullOrWhiteSpace(createdTo);
        bool hasDue = !string.IsNullOrWhiteSpace(dueFrom) || !string.IsNullOrWhiteSpace(dueTo);

        int kinds = (hasTitle ? 1 : 0) + (hasCreated ? 1 : 0) + (hasDue ? 1 : 0);

        if (kinds == 0)
        {
            return this.BadRequest("Provide exactly one of: title OR createdFrom/createdTo OR dueFrom/dueTo.");
        }

        if (kinds > 1)
        {
            return this.BadRequest("Do not mix criteria. Use only one kind: title OR created dates OR due dates.");
        }

        static bool TryParseDate(string? s, out DateTime? d)
        {
            d = null;
            if (string.IsNullOrWhiteSpace(s))
            {
                return true;
            }

            if (DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed))
            {
                d = parsed.Date;
                return true;
            }

            return false;
        }

        if (!TryParseDate(createdFrom, out var cFrom) ||
            !TryParseDate(createdTo, out var cTo) ||
            !TryParseDate(dueFrom, out var dFrom) ||
            !TryParseDate(dueTo, out var dTo))
        {
            return this.BadRequest("Dates must be in YYYY-MM-DD format.");
        }

        var (p, s) = this.ResolvePaging(page, pageSize);

        var (items, total) = await this.service.SearchAsync(
            hasTitle ? title : null,
            cFrom,
            cTo,
            dFrom,
            dTo,
            p,
            s);

        this.Response.Headers["X-Total-Count"] = total.ToString(CultureInfo.InvariantCulture);
        return this.Ok(items);
    }

    [HttpGet("tasks/{id:int}/tags")]
    public async Task<ActionResult<IReadOnlyList<TagModel>>> GetTagsForTask(int id, [FromServices] ITagDatabaseService tags)
    {
        var validationResult = this.ValidateGetTagsForTask(id, tags);
        if (validationResult is not null)
        {
            return validationResult;
        }

        return await this.GetTagsForTaskCoreAsync(id, tags);
    }

    [HttpPost("tasks/{id:int}/tags/{tagId:int}")]
    public async Task<IActionResult> AddTag(int id, int tagId, [FromServices] ITagDatabaseService tags)
    {
        var validationResult = this.ValidateAddTag(id, tagId, tags);
        if (validationResult is not null)
        {
            return validationResult;
        }

        return await this.AddTagCoreAsync(id, tagId, tags);
    }

    [HttpDelete("tasks/{id:int}/tags/{tagId:int}")]
    public async Task<IActionResult> RemoveTag(int id, int tagId, [FromServices] ITagDatabaseService tags)
    {
        var validation = this.ValidateRemoveTag(id, tagId, tags);
        if (validation is not null)
        {
            return validation;
        }

        return await this.RemoveTagCoreAsync(id, tagId, tags);
    }

    private ActionResult<IReadOnlyList<TagModel>>? ValidateGetTagsForTask(int id, ITagDatabaseService tags)
    {
        ArgumentNullException.ThrowIfNull(tags);

        if (id <= 0)
        {
            return this.BadRequest("Invalid task id");
        }

        return null;
    }

    private async Task<ActionResult<IReadOnlyList<TagModel>>> GetTagsForTaskCoreAsync(int id, ITagDatabaseService tags)
    {
        try
        {
            var result = await tags.GetTagsForTaskAsync(id);
            return this.Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return this.NotFound();
        }
    }

    private BadRequestObjectResult? ValidateRemoveTag(int id, int tagId, ITagDatabaseService tags)
    {
        ArgumentNullException.ThrowIfNull(tags);

        if (id <= 0 || tagId <= 0)
        {
            return this.BadRequest("Invalid id.");
        }

        return null;
    }

    private async Task<IActionResult> RemoveTagCoreAsync(int id, int tagId, ITagDatabaseService tags)
    {
        try
        {
            await tags.RemoveTagFromTaskAsync(id, tagId);
            return this.NoContent();
        }
        catch (KeyNotFoundException)
        {
            return this.NotFound();
        }
    }

    private (int Page, int PageSize) ResolvePaging(int page, int pageSize)
    {
        var def = this.configuration.GetValue<int>("Paging:DefaultPageSize");
        var max = this.configuration.GetValue<int>("Paging:MaxPageSize");

        pageSize = pageSize <= 0 ? def : Math.Min(pageSize, max);
        page = page <= 0 ? 1 : page;

        return (page, pageSize);
    }

    private string CurrentUserId()
    {
        return this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-user";
    }

    private BadRequestObjectResult? ValidateAddTag(int id, int tagId, ITagDatabaseService tags)
    {
        ArgumentNullException.ThrowIfNull(tags);

        if (id <= 0 || tagId <= 0)
        {
            return this.BadRequest("Invalid id.");
        }

        return null;
    }

    private async Task<IActionResult> AddTagCoreAsync(int id, int tagId, ITagDatabaseService tags)
    {
        try
        {
            await tags.AddTagToTaskAsync(id, tagId);
            return this.NoContent();
        }
        catch (KeyNotFoundException)
        {
            return this.NotFound();
        }
    }
}
