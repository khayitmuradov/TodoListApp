using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApi.Models;
using TodoListApp.WebApi.Services;

namespace TodoListApp.WebApi.Controller;

[ApiController]
[Route("api/tags")]
public class TagController : ControllerBase
{
    private readonly ITagDatabaseService tags;

    public TagController(ITagDatabaseService tags) => this.tags = tags;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TagModel>>> GetAll()
    {
        return this.Ok(await this.tags.GetAllAsync());
    }

    [HttpPost]
    public async Task<ActionResult<TagModel>> Create([FromBody] CreateTagModel model)
    {
        var t = await this.tags.CreateAsync(model);
        return this.CreatedAtAction(nameof(this.GetAll), new { id = t.Id }, t);
    }

    [HttpDelete("{tagId:int}")]
    public async Task<IActionResult> Delete(int tagId)
    {
        if (tagId <= 0)
        {
            return this.BadRequest("Invalid tagId");
        }

        try
        {
            await this.tags.DeleteAsync(tagId);
        }
        catch (KeyNotFoundException)
        {
            return this.NotFound();
        }

        return this.NoContent();
    }

    [HttpGet("{tagId:int}/tasks")]
    public async Task<ActionResult<IReadOnlyList<TaskModel>>> GetTasksByTag(int tagId)
    {
        if (tagId <= 0)
        {
            return this.BadRequest("Invalid tagId");
        }

        try
        {
            _ = this.Ok(await this.tags.GetTasksByTagAsync(tagId));
        }
        catch (KeyNotFoundException)
        {
            return this.NotFound();
        }

        return this.NotFound();
    }
}
