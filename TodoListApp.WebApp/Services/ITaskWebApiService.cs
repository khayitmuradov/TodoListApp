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
<<<<<<< HEAD

    Task<(IReadOnlyList<TaskItem> Items, int Total)> GetAssignedAsync(
    string? status, string sortBy, string order, int page, int pageSize, CancellationToken ct = default);
=======
>>>>>>> fd8379b471fe3521632245d5ea5df5160c1e08d5
}
