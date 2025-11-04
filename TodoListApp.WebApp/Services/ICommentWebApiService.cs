using TodoListApp.WebApp.Models;

namespace TodoListApp.WebApp.Services;

public interface ICommentWebApiService
{
    Task<IReadOnlyList<CommentItem>> GetForTaskAsync(int taskId, CancellationToken ct = default);

    Task<CommentItem> CreateAsync(int taskId, CreateCommentRequest req, CancellationToken ct = default);

    Task UpdateAsync(int taskId, int commentId, UpdateCommentRequest req, CancellationToken ct = default);

    Task DeleteAsync(int taskId, int commentId, CancellationToken ct = default);
}
