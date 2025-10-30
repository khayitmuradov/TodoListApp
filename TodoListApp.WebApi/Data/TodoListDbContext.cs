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

    public DbSet<TaskEntity> Tasks => this.Set<TaskEntity>();

    public DbSet<TagEntity> Tags => this.Set<TagEntity>();

    public DbSet<TaskTagEntity> TaskTags => this.Set<TaskTagEntity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        _ = builder.Entity<TaskEntity>()
            .HasIndex(t => new
            {
                t.TodoListId,
                t.Status,
            });

        _ = builder.Entity<TaskEntity>()
            .HasOne(t => t.TodoList)
            .WithMany(l => l.Tasks)
            .HasForeignKey(t => t.TodoListId)
            .OnDelete(DeleteBehavior.Cascade);

        _ = builder.Entity<TaskEntity>()
            .HasIndex(t => t.Title);

        _ = builder.Entity<TaskEntity>()
            .HasIndex(t => t.CreatedDate);

        _ = builder.Entity<TaskEntity>()
            .HasIndex(t => t.DueDate);

        _ = builder.Entity<TagEntity>()
            .HasIndex(t => t.Name);

        _ = builder.Entity<TaskTagEntity>()
            .HasKey(tt => new { tt.TaskId, tt.TagId });

        _ = builder.Entity<TaskTagEntity>()
            .HasOne(tt => tt.Task)
            .WithMany(t => t.TaskTags)
            .HasForeignKey(tt => tt.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        _ = builder.Entity<TaskTagEntity>()
            .HasOne(tt => tt.Tag)
            .WithMany(t => t.TaskTags)
            .HasForeignKey(tt => tt.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
