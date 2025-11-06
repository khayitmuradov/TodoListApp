using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TodoListApp.WebApp.Models;

namespace TodoListApp.WebApp.Services;

internal class TodoListWebApiService : ITodoListWebApiService
{
    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    public TodoListWebApiService(HttpClient httpClient, IConfiguration cfg)
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

    public async Task<(IReadOnlyList<TodoListModel> Items, int Total)> GetAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var uri = new Uri(this.httpClient.BaseAddress!, $"api/lists?page={page}&pageSize={pageSize}");
        var resp = await this.httpClient.GetAsync(uri, ct);
        _ = resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync(ct);
        var apiItems = JsonSerializer.Deserialize<List<TodoListWebApiModel>>(json, this.jsonOptions) ?? new List<TodoListWebApiModel>();
        var totalHeader = resp.Headers.TryGetValues("X-Total-Count", out var vals) ? vals.FirstOrDefault() : null;
        _ = int.TryParse(totalHeader, out var total);

        var items = apiItems.Select(x => new TodoListModel { Id = x.Id, Title = x.Title, Description = x.Description }).ToList();
        return (items, total);
    }

    public async Task<TodoListModel> CreateAsync(string title, string? description, CancellationToken ct = default)
    {
        var uri = new Uri(this.httpClient.BaseAddress!, "api/lists");
        var payload = new CreateTodoListWebApiModel { Title = title, Description = description };
        using var content = new StringContent(JsonSerializer.Serialize(payload, this.jsonOptions), Encoding.UTF8, "application/json");

        var resp = await this.httpClient.PostAsync(uri, content, ct);
        _ = resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync(ct);
        var created = JsonSerializer.Deserialize<TodoListWebApiModel>(json, this.jsonOptions) ?? throw new JsonException("TodoList deserialization returned null");
        return new TodoListModel { Id = created.Id, Title = created.Title, Description = created.Description };
    }

    public async Task UpdateAsync(int id, string title, string? description, CancellationToken ct = default)
    {
        var uri = new Uri(this.httpClient.BaseAddress!, $"api/lists/{id}");
        var payload = new UpdateTodoListWebApiModel { Title = title, Description = description };
        using var content = new StringContent(JsonSerializer.Serialize(payload, this.jsonOptions), Encoding.UTF8, "application/json");

        var resp = await this.httpClient.PutAsync(uri, content, ct);
        _ = resp.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var uri = new Uri(this.httpClient.BaseAddress!, $"api/lists/{id}");
        var resp = await this.httpClient.DeleteAsync(uri, ct);
        _ = resp.EnsureSuccessStatusCode();
    }
}
