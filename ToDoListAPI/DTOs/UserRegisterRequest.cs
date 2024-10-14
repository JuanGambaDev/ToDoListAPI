using System.ComponentModel.DataAnnotations;

namespace ToDoListAPI.DTOs;

public class UserRegisterRequest
{

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; }

        [Required]
        [MaxLength(200)]
        public string Password { get; set; } 
}