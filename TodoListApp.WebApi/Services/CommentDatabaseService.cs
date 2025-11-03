using Microsoft.EntityFrameworkCore;
using TodoListApp.WebApi.Data;
using TodoListApp.WebApi.Domain;
using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApi.Services;

internal class CommentDatabaseService : ICommentDatabaseService
{
    private readonly TodoListDbContext db;
    private readonly ILogger<CommentDatabaseService> logger;

    public CommentDatabaseService(
        TodoListDbContext db,
        ILogger<CommentDatabaseService> logger)
    {
        this.db = db;
        this.logger = logger;
    }

    public async Task<IReadOnlyList<CommentModel>> GetByTaskAsync(int taskId, string requesterId)
    {
        var task = await this.db.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == taskId)
            ?? throw new KeyNotFoundException();

        await this.EnsureListOwnerAsync(task.TodoListId, requesterId);

        var comments = await this.db.TaskComments.AsNoTracking()
            .Where(c => c.TaskId == taskId)
            .OrderByDescending(c => c.CreatedUtc)
            .ToListAsync();

        return comments.Select(ToModel).ToList();
    }

    public async Task<CommentModel> CreateAsync(int taskId, string requesterId, CreateCommentModel model)
    {
        var task = await this.db.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == taskId)
            ?? throw new KeyNotFoundException();

        await this.EnsureListOwnerAsync(task.TodoListId, requesterId);

        var entity = new TaskCommentEntity
        {
            TaskId = taskId,
            Text = model.Text,
            CreatedByUserId = requesterId,
            CreatedUtc = DateTime.UtcNow,
        };

        _ = this.db.TaskComments.Add(entity);
        _ = await this.db.SaveChangesAsync();

        this.logger.LogInformation("Comment {CommentId} created on task {TaskId} by {User}", entity.Id, taskId, requesterId);

        return ToModel(entity);
    }

    public async Task UpdateAsync(int taskId, int commentId, string requesterId, UpdateCommentModel model)
    {
        var comment = await this.db.TaskComments.FirstOrDefaultAsync(c => c.Id == commentId && c.TaskId == taskId)
            ?? throw new KeyNotFoundException();

        if (!string.Equals(comment.CreatedByUserId, requesterId, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException();
        }

        comment.Text = model.Text;
        comment.UpdatedUtc = DateTime.UtcNow;

        _ = await this.db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int taskId, int commentId, string requesterId)
    {
        var comment = await this.db.TaskComments.FirstOrDefaultAsync(c => c.Id == commentId && c.TaskId == taskId)
            ?? throw new KeyNotFoundException();

        if (!string.Equals(comment.CreatedByUserId, requesterId, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException();
        }

        _ = this.db.TaskComments.Remove(comment);
        _ = await this.db.SaveChangesAsync();
    }

    private static CommentModel ToModel(TaskCommentEntity c)
    {
        return new CommentModel
        {
            Id = c.Id,
            TaskId = c.TaskId,
            Text = c.Text,
            CreatedByUserId = c.CreatedByUserId,
            CreatedUtc = c.CreatedUtc,
            UpdatedUtc = c.UpdatedUtc,
        };
    }

    private async Task EnsureListOwnerAsync(int listId, string requesterId)
    {
        var list = await this.db.TodoLists.AsNoTracking().FirstOrDefaultAsync(l => l.Id == listId)
            ?? throw new KeyNotFoundException();

        if (!string.Equals(list.OwnerId, requesterId, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException();
        }
    }
}
