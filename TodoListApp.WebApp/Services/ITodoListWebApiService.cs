using TodoListApp.WebApp.Models;

namespace TodoListApp.WebApp.Services;

public interface ITodoListWebApiService
{
    Task<(IReadOnlyList<TodoList> Items, int Total)> GetAsync(int page, int pageSize, CancellationToken ct = default);

    Task<TodoList> CreateAsync(string title, string? description, CancellationToken ct = default);

    Task UpdateAsync(int id, string title, string? description, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}
