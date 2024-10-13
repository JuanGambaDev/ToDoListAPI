using System.ComponentModel.DataAnnotations;

namespace ToDoListAPI.Models;

public class ToDoItem {
    [Key]
    public int ToDoItemId { get; set;}
    [Required]
    [MaxLength(100)]
    public string Name { get; set;}
    [Required]
    [MaxLength(200)]
    public string Description { get; set;}
}
