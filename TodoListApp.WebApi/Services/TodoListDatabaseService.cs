using Microsoft.EntityFrameworkCore;
using TodoListApp.WebApi.Data;
using TodoListApp.WebApi.Domain;
using TodoListApp.WebApi.Services.Models;

namespace TodoListApp.WebApi.Services;

internal partial class TodoListDatabaseService : ITodoListDatabaseService
{
    private static readonly Action<ILogger, int, string, Exception?> LogTodoListCreated =
        LoggerMessage.Define<int, string>(
            LogLevel.Information,
            new EventId(1, nameof(LogTodoListCreated)),
            "TodoList created {TodoListId} by {OwnerId}");

    private readonly TodoListDbContext dbContext;
    private readonly ILogger<TodoListDatabaseService> logger;

    public TodoListDatabaseService(TodoListDbContext dbContext, ILogger<TodoListDatabaseService> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task<TodoList> CreateAsync(string ownerId, string title, string? description)
    {
        var e = new TodoListEntity
        {
            OwnerId = ownerId,
            Title = title,
            Description = description,
        };

        _ = this.dbContext.Add(e);
        _ = await this.dbContext.SaveChangesAsync();
        LogTodoListCreated(this.logger, e.Id, ownerId, null);

        return new TodoList
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            OwnerId = e.OwnerId,
        };
    }

    public async Task DeleteAsync(string requesterId, int id)
    {
        var e = await this.dbContext.TodoLists.FirstOrDefaultAsync(x => x.Id == id);
        if (e is null)
        {
            throw new KeyNotFoundException();
        }

        if (e.OwnerId != requesterId)
        {
            throw new UnauthorizedAccessException();
        }

        _ = this.dbContext.TodoLists.Remove(e);
        _ = await this.dbContext.SaveChangesAsync();
    }

    public async Task<(IReadOnlyList<TodoList> Items, int Total)> GetMineAsync(string userId, int page, int pageSize)
    {
        var query = this.dbContext.TodoLists.AsNoTracking().Where(x => x.OwnerId == userId);

        var total = await query.CountAsync();
        var entities = await query.OrderBy(x => x.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = entities.Select(e => new TodoList
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            OwnerId = e.OwnerId,
        }).ToList();

        return (items, total);
    }

    public async Task UpdateAsync(string requesterId, int id, string title, string? description)
    {
        var e = await this.dbContext.TodoLists.FirstOrDefaultAsync(x => x.Id == id);
        if (e == null)
        {
            throw new KeyNotFoundException();
        }

        if (e.OwnerId != requesterId)
        {
            throw new UnauthorizedAccessException();
        }

        e.Title = title;
        e.Description = description;
        e.UpdatedUtc = DateTime.UtcNow;

        _ = await this.dbContext.SaveChangesAsync();
    }
}
