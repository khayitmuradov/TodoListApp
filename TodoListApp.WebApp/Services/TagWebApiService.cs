using System.Text;
using System.Text.Json;
using TodoListApp.WebApp.Models;

namespace TodoListApp.WebApp.Services;

public class TagWebApiService : ITagWebApiService
{
    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    public TagWebApiService(HttpClient httpClient, IConfiguration configuration)
    {
        this.httpClient = httpClient;
        var baseUrl = configuration.GetValue<string>("Api:BaseUrl");
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            this.httpClient.BaseAddress = new Uri(baseUrl);
        }
    }

    public async Task<IReadOnlyList<TagItem>> GetAllAsync(CancellationToken ct = default)
    {
        var uri = new Uri(this.httpClient.BaseAddress!, "api/tags");
        var resp = await this.httpClient.GetAsync(uri, ct);
        _ = resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<List<TagItem>>(json, this.jsonOptions) ?? new List<TagItem>();
    }

    public async Task<TagItem> CreateAsync(CreateTagRequest req, CancellationToken ct = default)
    {
        var uri = new Uri(this.httpClient.BaseAddress!, "api/tags");
        using var content = new StringContent(JsonSerializer.Serialize(req, this.jsonOptions), Encoding.UTF8, "application/json");
        var resp = await this.httpClient.PostAsync(uri, content, ct);
        _ = resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync(ct);

        return JsonSerializer.Deserialize<TagItem>(json, this.jsonOptions) ?? new TagItem();
    }

    public async Task DeleteAsync(int tagId, CancellationToken ct = default)
    {
        var uri = new Uri(this.httpClient.BaseAddress!, $"api/tags/{tagId}");
        var resp = await this.httpClient.DeleteAsync(uri, ct);

        _ = resp.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<TaskItem>> GetTasksByTagAsync(int tagId, CancellationToken ct = default)
    {
        var uri = new Uri(this.httpClient.BaseAddress!, $"api/tags/{tagId}/tasks");
        var resp = await this.httpClient.GetAsync(uri, ct);
        _ = resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync(ct);

        return JsonSerializer.Deserialize<List<TaskItem>>(json, this.jsonOptions) ?? new List<TaskItem>();
    }

    public async Task<IReadOnlyList<TagItem>> GetTagsForTaskAsync(int taskId, CancellationToken ct = default)
    {
        var uri = new Uri(this.httpClient.BaseAddress!, $"api/tasks/{taskId}/tags");
        var resp = await this.httpClient.GetAsync(uri, ct);
        _ = resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync(ct);

        return JsonSerializer.Deserialize<List<TagItem>>(json, this.jsonOptions) ?? new List<TagItem>();
    }

    public async Task AddTagToTaskAsync(int taskId, int tagId, CancellationToken ct = default)
    {
        var uri = new Uri(this.httpClient.BaseAddress!, $"api/tasks/{taskId}/tags/{tagId}");
        var resp = await this.httpClient.PostAsync(uri, content: null, ct);

        _ = resp.EnsureSuccessStatusCode();
    }

    public async Task RemoveTagFromTaskAsync(int taskId, int tagId, CancellationToken ct = default)
    {
        var uri = new Uri(this.httpClient.BaseAddress!, $"api/tasks/{taskId}/tags/{tagId}");
        var resp = await this.httpClient.DeleteAsync(uri, ct);

        _ = resp.EnsureSuccessStatusCode();
    }
}
