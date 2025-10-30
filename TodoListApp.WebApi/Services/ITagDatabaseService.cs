using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApi.Services;

public interface ITagDatabaseService
{
    Task<TagModel> CreateAsync(CreateTagModel model);

    Task DeleteAsync(int tagId);

    Task<IReadOnlyList<TagModel>> GetAllAsync();

    Task<IReadOnlyList<TagModel>> GetTagsForTaskAsync(int taskId);

    Task AddTagToTaskAsync(int taskId, int tagId);

    Task RemoveTagFromTaskAsync(int taskId, int tagId);

    Task<IReadOnlyList<TaskModel>> GetTasksByTagAsync(int tagId);
}
