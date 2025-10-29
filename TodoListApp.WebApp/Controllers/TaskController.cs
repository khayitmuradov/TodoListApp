using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApp.Models;
using TodoListApp.WebApp.Services;
using TaskStatus = TodoListApp.WebApp.Models.TaskStatus;

namespace TodoListApp.WebApp.Controllers;

public class TaskController : Controller
{
    private readonly ITaskWebApiService tasks;

    public TaskController(ITaskWebApiService tasks) => this.tasks = tasks;

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        return this.View(await this.tasks.GetByIdAsync(id));
    }

    [HttpGet]
    public IActionResult Create(int listId)
    {
        this.ViewData["ListId"] = listId;
        return this.View(new CreateTaskRequest { Status = TaskStatus.NotStarted });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int listId, CreateTaskRequest model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (string.IsNullOrWhiteSpace(model.Title))
        {
            this.ModelState.AddModelError(nameof(model.Title), "Title is required.");
        }

        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        _ = await this.tasks.CreateAsync(listId, model);
        this.TempData["Message"] = "Task created.";

        return this.RedirectToAction("Index", "TodoList", new { selectedId = listId, taskPage = 1, taskPageSize = 5 });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, int listId)
    {
        var t = await this.tasks.GetByIdAsync(id);
        this.ViewData["ListId"] = listId;

        return this.View(new UpdateTaskRequest
        {
            Title = t.Title,
            Description = t.Description,
            DueDate = t.DueDate,
            Status = t.Status ?? TaskStatus.NotStarted,
            AssigneeId = t.AssigneeId,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateTaskRequest model, int listId)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (string.IsNullOrWhiteSpace(model.Title))
        {
            this.ModelState.AddModelError(nameof(model.Title), "Title is required.");
        }

        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        await this.tasks.UpdateAsync(id, model);
        this.TempData["Message"] = "Task updated.";

        return this.RedirectToAction("Index", "TodoList", new { selectedId = listId });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, int listId)
    {
        var t = await this.tasks.GetByIdAsync(id);
        this.ViewData["ListId"] = listId;
        return this.View(t);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, int listId)
    {
        await this.tasks.DeleteAsync(id);
        this.TempData["Message"] = "Task deleted.";
        return this.RedirectToAction("Index", "TodoList", new { selectedId = listId });
    }

    [HttpGet]
    public async Task<IActionResult> ChangeStatus(int id, int listId)
    {
        var t = await this.tasks.GetByIdAsync(id);
        this.ViewData["ListId"] = listId;
        this.ViewData["TaskId"] = id;
        return this.View(t);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, int listId, TaskStatus status)
    {
        await this.tasks.PatchStatusAsync(id, status);
        this.TempData["Message"] = "Status updated.";
        return this.RedirectToAction("Index", "TodoList", new { selectedId = listId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatusAssigned(
    int id,
    TaskStatus status,
    string aStatus = "InProgress",
    string aOrder = "asc",
    int aPage = 1,
    int aPageSize = 5,
    int? selectedId = null,
    string? selectedTitle = null,
    string? selectedDescription = null,
    int page = 1,
    int pageSize = 10,
    int taskPage = 1,
    int taskPageSize = 5)
    {
        await this.tasks.PatchStatusAsync(id, status);
        this.TempData["Message"] = "Status updated.";

        return this.RedirectToAction("Index", "TodoList", new
        {
            page,
            pageSize,
            selectedId,
            selectedTitle,
            selectedDescription,
            taskPage,
            taskPageSize,
            aStatus,
            aOrder,
            aPage,
            aPageSize,
        });
    }
}
