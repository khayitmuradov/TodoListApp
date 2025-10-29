using TodoListApp.WebApp.Models;
using TaskStatus = TodoListApp.WebApp.Models.TaskStatus;

namespace TodoListApp.WebApp.Services;

public interface ITaskWebApiService
{
    Task<(IReadOnlyList<TaskItem> Items, int Total)> GetByListAsync(int listId, int page, int pageSize, CancellationToken ct = default);

    Task<TaskDetails> GetByIdAsync(int id, CancellationToken ct = default);

    Task<TaskDetails> CreateAsync(int listId, CreateTaskRequest req, CancellationToken ct = default);

    Task UpdateAsync(int id, UpdateTaskRequest req, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);

    Task PatchStatusAsync(int id, TaskStatus status, CancellationToken ct = default);
}
