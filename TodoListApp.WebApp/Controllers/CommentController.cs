using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApp.Models;
using TodoListApp.WebApp.Services;

namespace TodoListApp.WebApp.Controllers;

public class CommentController : Controller
{
    private readonly ICommentWebApiService comments;

    public CommentController(ICommentWebApiService comments)
    {
        this.comments = comments;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int taskId, string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            _ = await this.comments.CreateAsync(taskId, new CreateCommentRequest { Text = text.Trim() });
        }

        return this.RedirectToAction("Details", "Task", new { id = taskId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int taskId, int commentId, string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            await this.comments.UpdateAsync(taskId, commentId, new UpdateCommentRequest { Text = text.Trim() });
        }

        return this.RedirectToAction("Details", "Task", new { id = taskId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int taskId, int commentId)
    {
        await this.comments.DeleteAsync(taskId, commentId);
        return this.RedirectToAction("Details", "Task", new { id = taskId });
    }
}
