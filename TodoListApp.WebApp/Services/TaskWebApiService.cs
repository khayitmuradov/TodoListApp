using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TodoListApp.WebApp.Models;
using TaskStatus = TodoListApp.WebApp.Models.TaskStatus;

namespace TodoListApp.WebApp.Services;

internal class TaskWebApiService : ITaskWebApiService
{
    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    public TaskWebApiService(HttpClient httpClient, IConfiguration cfg)
    {
        this.httpClient = httpClient;
        var baseUrl = cfg.GetValue<string>("Api:BaseUrl");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("Api:BaseUrl is missing in appsettings.json");
        }

        httpClient.BaseAddress = new Uri(baseUrl);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<(IReadOnlyList<TaskItem> Items, int Total)> GetByListAsync(int listId, int page, int pageSize, CancellationToken ct = default)
    {
        var uri = new Uri(this.httpClient.BaseAddress!, $"api/lists/{listId}/tasks?page={page}&pageSize={pageSize}");
        var resp = await this.httpClient.GetAsync(uri, ct);
        _ = resp.EnsureSuccessStatusCode();

        var body = await resp.Content.ReadAsStringAsync(ct);
        var items = JsonSerializer.Deserialize<List<TaskItem>>(body, this.jsonOptions) ?? new List<TaskItem>();

        var totalHeader = resp.Headers.TryGetValues("X-Total-Count", out var vals) ? vals.FirstOrDefault() : null;
        _ = int.TryParse(totalHeader, out var total);

        return (items, total);
    }

    public async Task<TaskDetails> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var uri = new Uri(this.httpClient.BaseAddress!, $"api/tasks/{id}");
        var resp = await this.httpClient.GetAsync(uri, ct);
        _ = resp.EnsureSuccessStatusCode();

        var body = await resp.Content.ReadAsStringAsync(ct);
        var item = JsonSerializer.Deserialize<TaskDetails>(body, this.jsonOptions) ?? throw new JsonException("TaskDetails deserialization returned null");
        return item;
    }

    public async Task<TaskDetails> CreateAsync(int listId, CreateTaskRequest req, CancellationToken ct = default)
    {
        var uri = new Uri(this.httpClient.BaseAddress!, $"api/lists/{listId}/tasks");
        using var content = new StringContent(JsonSerializer.Serialize(req, this.jsonOptions), Encoding.UTF8, "application/json");
        var resp = await this.httpClient.PostAsync(uri, content, ct);
        _ = resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<TaskDetails>(json, this.jsonOptions) ?? throw new JsonException("TaskDetails deserialization returned null");
    }

    public async Task UpdateAsync(int id, UpdateTaskRequest req, CancellationToken ct = default)
    {
        var uri = new Uri(this.httpClient.BaseAddress!, $"api/tasks/{id}");
        using var content = new StringContent(JsonSerializer.Serialize(req, this.jsonOptions), Encoding.UTF8, "application/json");
        var resp = await this.httpClient.PutAsync(uri, content, ct);
        _ = resp.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var uri = new Uri(this.httpClient.BaseAddress!, $"api/tasks/{id}");
        var resp = await this.httpClient.DeleteAsync(uri, ct);
        _ = resp.EnsureSuccessStatusCode();
    }

    public async Task PatchStatusAsync(int id, TaskStatus status, CancellationToken ct = default)
    {
        var uri = new Uri(this.httpClient.BaseAddress!, $"api/tasks/{id}/status");
        var req = new PatchTaskStatusRequest { Status = status };
        using var content = new StringContent(JsonSerializer.Serialize(req, this.jsonOptions), Encoding.UTF8, "application/json");
        var resp = await this.httpClient.PatchAsync(uri, content, ct);
        _ = resp.EnsureSuccessStatusCode();
    }
}
