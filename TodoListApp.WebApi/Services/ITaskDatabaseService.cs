using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApi.Services;

public interface ITaskDatabaseService
{
    Task<(IReadOnlyList<TaskModel> Items, int Total)> GetByListAsync(int listId, string requesterId, int page, int pageSize);

    Task<TaskModel> GetByIdAsync(int taskId, string requesterId);

    Task<TaskModel> CreateAsync(int listId, string creatorUserId, CreateTaskModel model);

    Task UpdateAsync(int taskId, string requesterId, UpdateTaskModel model);

    Task DeleteAsync(int taskId, string requesterId);

    Task<(IReadOnlyList<TaskModel> Items, int Total)> GetAssignedToMeAsync(
        string userId,
        string? statusFilter,
        string sortBy,
        string order,
        int page,
        int pageSize);

    Task ChangeStatusAsync(int taskId, string requesterId, Constraints.TaskStatus newStatus);
}
