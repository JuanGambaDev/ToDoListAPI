using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToDoListAPI.Models;

public class ToDoItem {

    [Key]
    public int ToDoItemId { get; set;}

    [Required]
    [MaxLength(100)]
    public string Title { get; set;}

    [Required]
    [MaxLength(200)]
    public string Description { get; set;}

    [ForeignKey(nameof(User))]
    [Required]    
    public int UserId { get; set;}
    public virtual User User { get; set;}
}
