using TodoListApp.WebApi.Services.Models;

namespace TodoListApp.WebApi.Services;

public interface ITodoListDatabaseService
{
    Task<(IReadOnlyList<TodoList> Items, int Total)> GetMineAsync(string userId, int page, int pageSize);

    Task<TodoList> CreateAsync(string ownerId, string title, string? description);

    Task UpdateAsync(string requesterId, int id, string title, string? description);

    Task DeleteAsync(string requesterId, int id);
}
