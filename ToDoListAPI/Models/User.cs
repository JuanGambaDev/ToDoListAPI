using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ToDoListAPI.Models
{
    public class User 
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; }

        [Required]
        [MaxLength(200)]
        public string PasswordHash { get; set; } 

        public virtual ICollection<ToDoItem>? ToDoItems { get; set; }  
    } 
}
