using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApi.Models;
using TodoListApp.WebApi.Services;

namespace TodoListApp.WebApi.Controller;

[ApiController]
[Route("api/lists")]
public class TodoListController : ControllerBase
{
    private readonly ITodoListDatabaseService service;
    private readonly IConfiguration configuration;

    public TodoListController(ITodoListDatabaseService service, IConfiguration configuration)
    {
        this.service = service;
        this.configuration = configuration;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoListModel>>> GetMine([FromQuery] int page = 1, int pageSize = 0)
    {
        var def = this.configuration.GetValue<int>("Paging:DefaultPageSize");
        var max = this.configuration.GetValue<int>("Paging:MaxPageSize");
        pageSize = pageSize <= 0 ? def : Math.Min(pageSize, max);

        var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-user";
        var (items, total) = await this.service.GetMineAsync(userId, page, pageSize);

        this.Response.Headers["X-Total-Count"] = total.ToString(CultureInfo.InvariantCulture);

        return this.Ok(items.Select(i => new TodoListModel
        {
            Id = i.Id,
            Title = i.Title,
            Description = i.Description,
        }));
    }

    [HttpPost]
    public async Task<ActionResult<TodoListModel>> Create([FromBody] CreateTodoListModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-user";
        if (model.Title == null)
        {
            return this.BadRequest("Title is required.");
        }

        var created = await this.service.CreateAsync(userId, model.Title, model.Description);

        return this.CreatedAtAction(nameof(this.GetMine), new { id = created.Id }, new TodoListModel
        {
            Id = created.Id,
            Title = created.Title,
            Description = created.Description,
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTodoListModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        if (model.Title == null)
        {
            return this.BadRequest("Title is required.");
        }

        try
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-user";
            await this.service.UpdateAsync(userId, id, model.Title, model.Description);
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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-user";
            await this.service.DeleteAsync(userId, id);
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
}
