using Microsoft.EntityFrameworkCore;
using TodoListApp.WebApi.Data;
using TodoListApp.WebApi.Domain;
using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApi.Services;

internal class TaskDatabaseService : ITaskDatabaseService
{
    private readonly TodoListDbContext dbContext;
    private readonly ILogger<TaskDatabaseService> logger;

    public TaskDatabaseService(TodoListDbContext dbContext, ILogger<TaskDatabaseService> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task<(IReadOnlyList<TaskModel> Items, int Total)> GetByListAsync(int listId, string requesterId, int page, int pageSize)
    {
        _ = await this.GetOwnedListAsync(listId, requesterId);

        var q = this.dbContext.Tasks.AsNoTracking().Where(t => t.TodoListId == listId);

        var total = await q.CountAsync();
        var tasks = await q.OrderBy(t => t.DueDate ?? DateTime.MaxValue)
                           .ThenBy(t => t.Title)
                           .Skip((page - 1) * pageSize)
                           .Take(pageSize)
                           .ToListAsync();

        return (tasks.Select(ToModel).ToList(), total);
    }

    public async Task<TaskModel> GetByIdAsync(int taskId, string requesterId)
    {
        var t = await this.dbContext.Tasks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == taskId)
            ?? throw new KeyNotFoundException();

        _ = await this.GetOwnedListAsync(t.TodoListId, requesterId); // ownership
        return ToModel(t);
    }

    public async Task<TaskModel> CreateAsync(int listId, string creatorUserId, CreateTaskModel model)
    {
        _ = await this.GetOwnedListAsync(listId, creatorUserId);

        var status = model.Status ?? Constraints.TaskStatus.NotStarted;

        var e = new TaskEntity
        {
            TodoListId = listId,
            Title = model.Title,
            Description = model.Description,
            DueDate = model.DueDate,
            Status = status,
            AssigneeId = creatorUserId,
            CreatedDate = DateTime.UtcNow,
        };

        _ = this.dbContext.Add(e);
        _ = await this.dbContext.SaveChangesAsync();
        this.logger.LogInformation("Task {TaskId} created in list {ListId} by {User}", e.Id, listId, creatorUserId);

        return ToModel(e);
    }

    public async Task DeleteAsync(int taskId, string requesterId)
    {
        var e = await this.dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == taskId)
                ?? throw new KeyNotFoundException();

        _ = await this.GetOwnedListAsync(e.TodoListId, requesterId);

        _ = this.dbContext.Tasks.Remove(e);
        _ = await this.dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(int taskId, string requesterId, UpdateTaskModel model)
    {
        var e = await this.dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == taskId)
                ?? throw new KeyNotFoundException();

        _ = await this.GetOwnedListAsync(e.TodoListId, requesterId);

        e.Title = model.Title;
        e.Description = model.Description;
        e.DueDate = model.DueDate;
        e.Status = model.Status;

        if (!string.IsNullOrWhiteSpace(model.AssigneeId))
        {
            e.AssigneeId = model.AssigneeId;
        }

        _ = await this.dbContext.SaveChangesAsync();
    }

    private static bool IsOverdue(TaskEntity t) =>
        t.DueDate.HasValue && t.DueDate.Value.Date < DateTime.UtcNow.Date && t.Status != Constraints.TaskStatus.Completed;

    private static TaskModel ToModel(TaskEntity t)
    {
        return new TaskModel
        {
            Id = t.Id,
            TodoListId = t.TodoListId,
            Title = t.Title,
            Description = t.Description,
            CreatedDate = t.CreatedDate,
            DueDate = t.DueDate,
            Status = t.Status,
            AssigneeId = t.AssigneeId,
            IsOverdue = IsOverdue(t),
        };
    }

    private async Task<TodoListEntity> GetOwnedListAsync(int listId, string requesterId)
    {
        var list = await this.dbContext.TodoLists.AsNoTracking().FirstOrDefaultAsync(l => l.Id == listId);
        if (list == null)
        {
            throw new KeyNotFoundException();
        }

        if (list.OwnerId != requesterId)
        {
            throw new UnauthorizedAccessException();
        }

        return list;
    }
}
