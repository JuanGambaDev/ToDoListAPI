using System.ComponentModel.DataAnnotations;

namespace ToDoListAPI.DTOs;

public class UserLoginRequest 
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; }

    [Required]
    [MaxLength(200)]
    public string PasswordHash { get; set; } 

}