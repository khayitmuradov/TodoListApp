using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApi.Models;
using TodoListApp.WebApi.Services;

namespace TodoListApp.WebApi.Controller;

[ApiController]
[Route("api/tasks/{taskId:int}/comments")]
public class TaskCommentsController : ControllerBase
{
    private readonly ICommentDatabaseService comments;
    private readonly ILogger<TaskCommentsController> logger;

    public TaskCommentsController(
        ICommentDatabaseService comments,
        ILogger<TaskCommentsController> logger)
    {
        this.comments = comments;
        this.logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CommentModel>>> GetAll(int taskId)
    {
        try
        {
            var items = await this.comments.GetByTaskAsync(taskId, this.CurrentUserId());
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

    [HttpPost]
    public async Task<ActionResult<CommentModel>> Create(int taskId, [FromBody] CreateCommentModel model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        try
        {
            var created = await this.comments.CreateAsync(taskId, this.CurrentUserId(), model);
            return this.CreatedAtAction(nameof(GetAll), new { taskId = taskId }, created);
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

    [HttpPut("{commentId:int}")]
    public async Task<IActionResult> Update(int taskId, int commentId, [FromBody] UpdateCommentModel model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        try
        {
            await this.comments.UpdateAsync(taskId, commentId, this.CurrentUserId(), model);
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

    [HttpDelete("{commentId:int}")]
    public async Task<IActionResult> Delete(int taskId, int commentId)
    {
        try
        {
            await this.comments.DeleteAsync(taskId, commentId, this.CurrentUserId());
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

    private string CurrentUserId()
    {
        return this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-user";
    }
}
