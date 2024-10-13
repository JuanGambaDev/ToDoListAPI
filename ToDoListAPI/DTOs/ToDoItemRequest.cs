using System.ComponentModel.DataAnnotations;

namespace ToDoListAPI.DTOs;

public class ToDoItemRequest  {

    [Required]
    [MaxLength(100)]
    public string Title { get; set;}

    [Required]
    [MaxLength(200)]
    public string Description { get; set;}
}