using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TodoListApp.WebApi.Domain;

namespace TodoListApp.WebApi.Data;

internal class TodoListDbContext : IdentityDbContext<IdentityUser>
{
    public TodoListDbContext(DbContextOptions<TodoListDbContext> options)
        : base(options)
    {
    }

    public DbSet<TodoListEntity> TodoLists => this.Set<TodoListEntity>();
}
