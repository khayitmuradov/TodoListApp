using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApp.Models;
using TodoListApp.WebApp.Services;

namespace TodoListApp.WebApp.Controllers;

public class SearchController : Controller
{
    private readonly ITaskWebApiService tasks;

    public SearchController(ITaskWebApiService tasks) => this.tasks = tasks;

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] TaskSearchQuery query, CancellationToken ct)
    {
        if (query is null)
        {
            return this.BadRequest("Query parameters cannot be null.");
        }

        if (query.PageSize <= 0)
        {
            query.PageSize = 10;
        }

        if (query.Page <= 0)
        {
            query.Page = 1;
        }

        var modes = (query.IsTitleMode ? 1 : 0) + (query.IsCreatedMode ? 1 : 0) + (query.IsDueMode ? 1 : 0);
        if (modes == 0)
        {
            return this.View(new TaskSearchResult
            {
                Items = Array.Empty<TaskItem>(),
                Total = 0,
                Page = query.Page,
                PageSize = query.PageSize,
                Query = query,
            });
        }

        if (modes > 1)
        {
            this.ModelState.AddModelError(string.Empty, "Use only one criterion at a time: Title OR Created dates OR Due dates.");
            return this.View(new TaskSearchResult
            {
                Items = Array.Empty<TaskItem>(),
                Total = 0,
                Page = query.Page,
                PageSize = query.PageSize,
                Query = query,
            });
        }

        var (items, total) = await this.tasks.SearchAsync(query, ct);

        return this.View(new TaskSearchResult
        {
            Items = items,
            Total = total,
            Page = query.Page,
            PageSize = query.PageSize,
            Query = query,
        });
    }
}
