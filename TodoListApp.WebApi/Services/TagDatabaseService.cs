using Microsoft.EntityFrameworkCore;
using TodoListApp.WebApi.Data;
using TodoListApp.WebApi.Domain;
using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApi.Services;

internal class TagDatabaseService : ITagDatabaseService
{
    private readonly TodoListDbContext db;

    public TagDatabaseService(TodoListDbContext db)
    {
        this.db = db;
    }

    public async Task<TagModel> CreateAsync(CreateTagModel model)
    {
        var name = ValidateCreateTagModel(model);
        return await this.CreateTagInternalAsync(name);
    }

    public async Task DeleteAsync(int tagId)
    {
        var tag = await this.db.Tags.FirstOrDefaultAsync(t => t.Id == tagId)
              ?? throw new KeyNotFoundException($"Tag {tagId} not found.");

        _ = this.db.Tags.Remove(tag);
        _ = await this.db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<TagModel>> GetAllAsync()
    {
        return await this.db.Tags
            .OrderBy(t => t.Name)
            .Select(t => new TagModel { Id = t.Id, Name = t.Name, ColorHex = t.ColorHex })
            .ToListAsync();
    }

    public async Task<IReadOnlyList<TagModel>> GetTagsForTaskAsync(int taskId)
    {
        _ = await this.db.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == taskId)
        ?? throw new KeyNotFoundException($"Task {taskId} not found.");

        return await this.db.TaskTags
            .Where(tt => tt.TaskId == taskId)
            .Select(tt => new TagModel
            {
                Id = tt.Tag.Id,
                Name = tt.Tag.Name,
                ColorHex = tt.Tag.ColorHex,
            })
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task AddTagToTaskAsync(int taskId, int tagId)
    {
        _ = await this.db.Tasks.FindAsync(taskId) ?? throw new KeyNotFoundException($"Task {taskId} not found.");
        _ = await this.db.Tags.FindAsync(tagId) ?? throw new KeyNotFoundException($"Tag {tagId} not found.");

        var exists = await this.db.Tasks.AnyAsync(x => x.Id == taskId)
                     && await this.db.Tags.AnyAsync(x => x.Id == tagId);
        if (!exists)
        {
            throw new KeyNotFoundException();
        }

        var linked = await this.db.TaskTags.FindAsync(taskId, tagId);
        if (linked == null)
        {
            _ = this.db.TaskTags.Add(new TaskTagEntity { TaskId = taskId, TagId = tagId });
            _ = await this.db.SaveChangesAsync();
        }
    }

    public async Task RemoveTagFromTaskAsync(int taskId, int tagId)
    {
        _ = await this.db.Tasks.FindAsync(taskId) ?? throw new KeyNotFoundException($"Task {taskId} not found.");
        _ = await this.db.Tags.FindAsync(tagId) ?? throw new KeyNotFoundException($"Tag {tagId} not found.");

        var link = await this.db.TaskTags.FindAsync(taskId, tagId)
                   ?? throw new KeyNotFoundException();
        _ = this.db.TaskTags.Remove(link);
        _ = await this.db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<TaskModel>> GetTasksByTagAsync(int tagId)
    {
        _ = await this.db.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tagId)
        ?? throw new KeyNotFoundException($"Tag {tagId} not found.");

        var q =
            from tt in this.db.TaskTags
            where tt.TagId == tagId
            select tt.Task;

        var tasks = await q
            .AsNoTracking()
            .OrderBy(t => t.DueDate ?? t.CreatedDate)
            .ToListAsync();

        var byTaskId = await this.db.TaskTags
            .Where(tt => tasks.Select(t => t.Id).Contains(tt.TaskId))
            .Include(tt => tt.Tag)
            .ToListAsync();

        return tasks.Select(t => new TaskModel
        {
            Id = t.Id,
            TodoListId = t.TodoListId,
            Title = t.Title,
            Description = t.Description,
            CreatedDate = t.CreatedDate,
            DueDate = t.DueDate,
            Status = t.Status,
            Tags = byTaskId.Where(x => x.TaskId == t.Id)
                           .Select(x => new TagModel { Id = x.TagId, Name = x.Tag.Name, ColorHex = x.Tag.ColorHex })
                           .OrderBy(x => x.Name)
                           .ToList(),
        }).ToList();
    }

    private static string ValidateCreateTagModel(CreateTagModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var name = (model.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Tag name is required");
        }

        return name;
    }

    private async Task<TagModel> CreateTagInternalAsync(string name)
    {
        var color = TagColor.FromName(name);
        var e = new TagEntity { Name = name, ColorHex = color };

        _ = this.db.Tags.Add(e);
        _ = await this.db.SaveChangesAsync();

        return new TagModel
        {
            Id = e.Id,
            Name = e.Name,
            ColorHex = e.ColorHex,
        };
    }
}
