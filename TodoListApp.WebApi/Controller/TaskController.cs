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
    private static readonly Action<ILogger, string, Exception?> LogBadCreateRequest =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(1001, nameof(LogBadCreateRequest)),
            "Bad create request: {ErrorMessage}");

    private readonly ITaskDatabaseService service;
    private readonly IConfiguration configuration;
    private readonly ILogger<TaskController> logger;

    public TaskController(
        ITaskDatabaseService service,
        IConfiguration configuration,
        ILogger<TaskController> logger)
    {
        this.service = service;
        this.configuration = configuration;
        this.logger = logger;
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
            LogBadCreateRequest(this.logger, ex.Message, ex);
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
        catch (AppException ex)
        {
            this.logger.LogError(ex, "Failed to get tasks assigned to current user");
            return this.StatusCode(500, "Internal Server Error");
        }
    }

    /// <summary>
    /// Change the status of a task. Allowed for assignee or list owner.
    /// </summary>
    [HttpPatch("tasks/{id:int}/status")]
    public async Task<IActionResult> ChangeStatusAsync(int id, [FromBody] ChangeTaskStatusModel model)
    {
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
}
