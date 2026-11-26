using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<IdentityUser>(options), IDataProtectionKeyContext
{
    public DbSet<TodoItem> Todos => Set<TodoItem>();
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<TodoItem>(todo =>
        {
            todo.HasKey(item => item.Id);
            todo.Property(item => item.Title).HasMaxLength(TodoItem.TitleMaxLength).IsRequired();
            todo.Property(item => item.OwnerId).IsRequired();
            todo.HasIndex(item => item.OwnerId);
        });
    }
}
