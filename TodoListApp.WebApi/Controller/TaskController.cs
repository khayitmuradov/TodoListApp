using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
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
