using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApp.Models;
using TodoListApp.WebApp.Services;

namespace TodoListApp.WebApp.Controllers;

public class TagController : Controller
{
    private readonly ITaskWebApiService tasks;
    private readonly ITagWebApiService tags;

    public TagController(ITaskWebApiService tasks, ITagWebApiService tags)
    {
        this.tasks = tasks;
        this.tags = tags;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            _ = await this.tags.CreateAsync(new CreateTagRequest { Name = name.Trim() });
        }

        return this.RedirectToAction("Index", "TodoList");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await this.tags.DeleteAsync(id);
        return this.RedirectToAction("Index", "TodoList");
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var task = await this.tasks.GetByIdAsync(id);
        var taskTags = await this.tags.GetTagsForTaskAsync(id);
        var allTags = await this.tags.GetAllAsync();

        this.ViewData["TaskTags"] = taskTags;
        this.ViewData["AllTags"] = allTags;

        return this.View(task);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTag(int id, int tagId)
    {
        await this.tags.AddTagToTaskAsync(id, tagId);
        return this.RedirectToAction(nameof(this.Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveTag(int id, int tagId)
    {
        await this.tags.RemoveTagFromTaskAsync(id, tagId);
        return this.RedirectToAction(nameof(this.Details), new { id });
    }
}
