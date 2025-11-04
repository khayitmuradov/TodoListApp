using System.Text;
using System.Text.Json;
using TodoListApp.WebApp.Models;

namespace TodoListApp.WebApp.Services;

public class CommentWebApiService : ICommentWebApiService
{
    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    public CommentWebApiService(HttpClient httpClient, IConfiguration configuration)
    {
        this.httpClient = httpClient;
        var baseUrl = configuration.GetValue<string>("Api:BaseUrl");
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            this.httpClient.BaseAddress = new Uri(baseUrl);
        }
    }

    public async Task<IReadOnlyList<CommentItem>> GetForTaskAsync(int taskId, CancellationToken ct = default)
    {
        var uri = new Uri(this.httpClient.BaseAddress!, $"api/tasks/{taskId}/comments");
        var resp = await this.httpClient.GetAsync(uri, ct);
        _ = resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<List<CommentItem>>(json, this.jsonOptions) ?? new List<CommentItem>();
    }

    public async Task<CommentItem> CreateAsync(int taskId, CreateCommentRequest req, CancellationToken ct = default)
    {
        var uri = new Uri(this.httpClient.BaseAddress!, $"api/tasks/{taskId}/comments");
        using var content = new StringContent(JsonSerializer.Serialize(req, this.jsonOptions), Encoding.UTF8, "application/json");
        var resp = await this.httpClient.PostAsync(uri, content, ct);
        _ = resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<CommentItem>(json, this.jsonOptions) ?? new CommentItem();
    }

    public async Task UpdateAsync(int taskId, int commentId, UpdateCommentRequest req, CancellationToken ct = default)
    {
        var uri = new Uri(this.httpClient.BaseAddress!, $"api/tasks/{taskId}/comments/{commentId}");
        using var content = new StringContent(JsonSerializer.Serialize(req, this.jsonOptions), Encoding.UTF8, "application/json");
        var resp = await this.httpClient.PutAsync(uri, content, ct);
        _ = resp.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int taskId, int commentId, CancellationToken ct = default)
    {
        var uri = new Uri(this.httpClient.BaseAddress!, $"api/tasks/{taskId}/comments/{commentId}");
        var resp = await this.httpClient.DeleteAsync(uri, ct);
        _ = resp.EnsureSuccessStatusCode();
    }
}
