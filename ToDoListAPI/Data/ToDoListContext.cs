using Microsoft.EntityFrameworkCore;
using ToDoListAPI.Models;

namespace ToDoListAPI.Data;

public class ToDoListContext : DbContext {

    public DbSet<ToDoItem> toDoItems { get; set; }

    public ToDoListContext(DbContextOptions<ToDoListContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {

        modelBuilder.Entity<ToDoItem>
        (
            ToDoItem =>
            {
                ToDoItem.ToTable("to_do_items");
                ToDoItem.HasKey(p => p.ToDoItemId);
                ToDoItem.Property(p => p.ToDoItemId).ValueGeneratedOnAdd();
                ToDoItem.Property(p => p.Title).IsRequired().HasMaxLength(100);
                ToDoItem.Property(p => p.Description).IsRequired().HasMaxLength(200);
            }
        );

    }

}