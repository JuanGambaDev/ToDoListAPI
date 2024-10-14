using Microsoft.EntityFrameworkCore;
using ToDoListAPI.Models;

namespace ToDoListAPI.Data;

public class ToDoListContext : DbContext {

    public DbSet<ToDoItem> ToDoItems { get; set; }
    public DbSet<User> Users { get; set; }

    public ToDoListContext(DbContextOptions<ToDoListContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {

        modelBuilder.Entity<ToDoItem>
        (
            ToDoItem =>
            {
                ToDoItem.ToTable("ToDoItems");
                ToDoItem.HasKey(p => p.ToDoItemId);
                ToDoItem.Property(p => p.ToDoItemId).ValueGeneratedOnAdd();
                ToDoItem.Property(p => p.Title).IsRequired().HasMaxLength(100);
                ToDoItem.Property(p => p.Description).IsRequired().HasMaxLength(200);
                ToDoItem.HasOne(t => t.User).WithMany(u => u.ToDoItems).HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
            }
        );

        modelBuilder.Entity<User>
        (
            User =>
            {
                User.ToTable("Users");
                User.HasKey(u => u.UserId);
                User.Property(u => u.UserId).ValueGeneratedOnAdd();
                User.Property(u => u.Name).IsRequired().HasMaxLength(100);
                User.Property(u => u.Email).IsRequired().HasMaxLength(256);
                User.HasIndex(e => e.Email).IsUnique();
                User.Property(u => u.PasswordHash).IsRequired().HasMaxLength(200);
            }
        );

    }
}