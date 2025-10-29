using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApp.Models;
using TodoListApp.WebApp.Services;

namespace TodoListApp.WebApp.Controllers;

public class TodoListController : Controller
{
    private readonly ITodoListWebApiService lists;
    private readonly ITaskWebApiService tasks;

    public TodoListController(ITodoListWebApiService lists, ITaskWebApiService tasks)
    {
        this.lists = lists;
        this.tasks = tasks;
    }

    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = 5,
        int? selectedId = null,
        string? selectedTitle = null,
        string? selectedDescription = null,
        int taskPage = 1,
        int taskPageSize = 5)
    {
        var (items, total) = await this.lists.GetAsync(page, pageSize);
        this.ViewData["Total"] = total;
        this.ViewData["Page"] = page;
        this.ViewData["PageSize"] = pageSize;

        this.ViewData["SelectedId"] = selectedId;
        this.ViewData["SelectedTitle"] = selectedTitle;
        this.ViewData["SelectedDescription"] = selectedDescription;

        if (selectedId.HasValue)
        {
            var (tItems, tTotal) = await this.tasks.GetByListAsync(selectedId.Value, taskPage, taskPageSize);
            this.ViewData["TaskItems"] = tItems;
            this.ViewData["TaskTotal"] = tTotal;
            this.ViewData["TaskPage"] = taskPage;
            this.ViewData["TaskPageSize"] = taskPageSize;
        }

        return this.View(items);
    }

    [HttpGet]
    public IActionResult Create() => this.View(new TodoList());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TodoList model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        if (string.IsNullOrWhiteSpace(model.Title))
        {
            this.ModelState.AddModelError(nameof(model.Title), "Title is required.");
            return this.View(model);
        }

        _ = await this.lists.CreateAsync(model.Title!, model.Description);
        this.TempData["Message"] = "List created.";
        return this.RedirectToAction(nameof(this.Index));
    }

    [HttpGet]
    public IActionResult Edit(int id, string? title, string? description)
        => this.View(new TodoList { Id = id, Title = title, Description = description });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TodoList model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        if (string.IsNullOrWhiteSpace(model.Title))
        {
            this.ModelState.AddModelError(nameof(model.Title), "Title is required.");
            return this.View(model);
        }

        await this.lists.UpdateAsync(model.Id, model.Title!, model.Description);
        this.TempData["Message"] = "List updated.";
        return this.RedirectToAction(nameof(this.Index));
    }

    [HttpGet]
    public IActionResult Delete(int id, string? title)
        => this.View(new TodoList { Id = id, Title = title });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await this.lists.DeleteAsync(id);
        this.TempData["Message"] = "List deleted.";
        return this.RedirectToAction(nameof(this.Index));
    }
}
