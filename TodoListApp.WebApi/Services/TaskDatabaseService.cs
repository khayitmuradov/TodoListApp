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

        _ = await this.GetOwnedListAsync(t.TodoListId, requesterId);
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

    public async Task<(IReadOnlyList<TaskModel> Items, int Total)> GetAssignedToMeAsync(
        string userId,
        string? statusFilter,
        string sortBy,
        string order,
        int page,
        int pageSize)
    {
        var q = this.dbContext.Tasks.AsNoTracking()
            .Where(t => t.AssigneeId == userId);

        if (string.IsNullOrWhiteSpace(statusFilter))
        {
            q = q.Where(t => t.Status == Constraints.TaskStatus.InProgress);
        }
        else if (Enum.TryParse<Constraints.TaskStatus>(statusFilter, true, out var parsed))
        {
            q = q.Where(t => t.Status == parsed);
        }

        var by = (sortBy ?? "dueDate").ToLowerInvariant();
        var ord = (order ?? "asc").ToLowerInvariant();

        IOrderedQueryable<TaskEntity> ordered = by switch
        {
            "name" => ord == "desc"
                ? q.OrderByDescending(t => t.Title)
                : q.OrderBy(t => t.Title),

            _ =>
                ord == "desc"
                ? q.OrderByDescending(t => t.DueDate ?? DateTime.MaxValue).ThenByDescending(t => t.Title)
                : q.OrderBy(t => t.DueDate ?? DateTime.MaxValue).ThenBy(t => t.Title),
        };

        var total = await ordered.CountAsync();

        var items = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items.Select(ToModel).ToList(), total);
    }

    public async Task ChangeStatusAsync(int taskId, string requesterId, Constraints.TaskStatus newStatus)
    {
        var e = await this.dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == taskId)
            ?? throw new KeyNotFoundException();

        if (e.AssigneeId != requesterId)
        {
            var list = await this.dbContext.TodoLists.AsNoTracking().FirstOrDefaultAsync(l => l.Id == e.TodoListId);

            if (list == null)
            {
                throw new KeyNotFoundException();
            }

            if (list.OwnerId != requesterId)
            {
                throw new UnauthorizedAccessException();
            }
        }

        e.Status = newStatus;
        _ = await this.dbContext.SaveChangesAsync();

        this.logger.LogInformation("Task {TaskId} status changed to {Status} by {User}", taskId, newStatus, requesterId);
    }

    public async Task<(IReadOnlyList<TaskModel> Items, int Total)> SearchAsync(
    string? title,
    DateTime? createdFrom, DateTime? createdTo,
    DateTime? dueFrom, DateTime? dueTo,
    int page, int pageSize)
    {
        // base query: ALL tasks (no owner restriction)
        IQueryable<TaskEntity> q = this.dbContext.Tasks.AsNoTracking();

        // exactly one kind of criterion will be set by controller validation
        if (!string.IsNullOrWhiteSpace(title))
        {
            // case-insensitive contains; SQL Server is often case-insensitive but Like is explicit
            var pattern = $"%{title.Trim()}%";
            q = q.Where(t => t.Title != null && EF.Functions.Like(t.Title, pattern));
        }
        else if (createdFrom.HasValue || createdTo.HasValue)
        {
            var from = createdFrom?.Date;
            // toExclusive = (createdTo + 1 day) to make inclusive date-only filtering
            var toExclusive = createdTo.HasValue ? createdTo.Value.Date.AddDays(1) : (DateTime?)null;

            if (from.HasValue)
            {
                q = q.Where(t => t.CreatedDate >= from.Value);
            }

            if (toExclusive.HasValue)
            {
                q = q.Where(t => t.CreatedDate < toExclusive.Value);
            }
        }
        else if (dueFrom.HasValue || dueTo.HasValue)
        {
            var from = dueFrom?.Date;
            var toExclusive = dueTo.HasValue ? dueTo.Value.Date.AddDays(1) : (DateTime?)null;

            if (from.HasValue)
            {
                q = q.Where(t => t.DueDate.HasValue && t.DueDate.Value >= from.Value);
            }

            if (toExclusive.HasValue)
            {
                q = q.Where(t => t.DueDate.HasValue && t.DueDate.Value < toExclusive.Value);
            }
        }

        var total = await q.CountAsync();

        // no sorting requested — apply paging directly
        var skip = (page - 1) * pageSize;
        var items = await q.Skip(skip).Take(pageSize).ToListAsync();

        var models = items.Select(ToModel).ToList();
        return (models, total);
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
