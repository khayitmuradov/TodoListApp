using TodoListApp.WebApp.Models;

namespace TodoListApp.WebApp.Services;

public interface ITagWebApiService
{
    Task<IReadOnlyList<TagItem>> GetAllAsync(CancellationToken ct = default);

    Task<TagItem> CreateAsync(CreateTagRequest req, CancellationToken ct = default);

    Task DeleteAsync(int tagId, CancellationToken ct = default);

    Task<IReadOnlyList<TaskItem>> GetTasksByTagAsync(int tagId, CancellationToken ct = default);

    Task<IReadOnlyList<TagItem>> GetTagsForTaskAsync(int taskId, CancellationToken ct = default);

    Task AddTagToTaskAsync(int taskId, int tagId, CancellationToken ct = default);

    Task RemoveTagFromTaskAsync(int taskId, int tagId, CancellationToken ct = default);
}
