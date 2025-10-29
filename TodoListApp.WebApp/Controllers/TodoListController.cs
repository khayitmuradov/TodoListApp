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
<<<<<<< HEAD
        int taskPageSize = 5,
        string aStatus = "InProgress",
        string aOrder = "asc",
        int aPage = 1,
        int aPageSize = 5)
=======
        int taskPageSize = 5)
>>>>>>> fd8379b471fe3521632245d5ea5df5160c1e08d5
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

<<<<<<< HEAD
        var (aItems, aTotal) = await this.tasks.GetAssignedAsync(aStatus, "dueDate", aOrder, aPage, aPageSize);
        this.ViewData["AItems"] = aItems;
        this.ViewData["ATotal"] = aTotal;
        this.ViewData["AStatus"] = aStatus;
        this.ViewData["AOrder"] = aOrder;
        this.ViewData["APage"] = aPage;
        this.ViewData["APageSize"] = aPageSize;

=======
>>>>>>> fd8379b471fe3521632245d5ea5df5160c1e08d5
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
