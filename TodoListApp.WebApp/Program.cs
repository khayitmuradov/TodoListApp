using TodoListApp.WebApp.Services;

namespace TodoListApp.WebApp;

internal static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        _ = builder.Services.AddControllersWithViews();

        _ = builder.Services.AddHttpClient<ITodoListWebApiService, TodoListWebApiService>();
        _ = builder.Services.AddHttpClient<ITaskWebApiService, TaskWebApiService>();
        _ = builder.Services.AddHttpClient<ITagWebApiService, TagWebApiService>();
        _ = builder.Services.AddHttpClient<ICommentWebApiService, CommentWebApiService>();

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            _ = app.UseExceptionHandler("/Home/Error");
            _ = app.UseHsts();
        }

        _ = app.UseHttpsRedirection();
        _ = app.UseStaticFiles();

        _ = app.UseRouting();

        _ = app.UseAuthorization();

        _ = app.MapControllerRoute(
            name: "default",
            pattern: "{controller=TodoList}/{action=Index}/{id?}");

        app.Run();
    }
}
