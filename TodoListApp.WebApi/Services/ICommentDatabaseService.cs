using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApi.Services;

public interface ICommentDatabaseService
{
    Task<IReadOnlyList<CommentModel>> GetByTaskAsync(int taskId, string requesterId);

    Task<CommentModel> CreateAsync(int taskId, string requesterId, CreateCommentModel model);

    Task UpdateAsync(int taskId, int commentId, string requesterId, UpdateCommentModel model);

    Task DeleteAsync(int taskId, int commentId, string requesterId);
}
