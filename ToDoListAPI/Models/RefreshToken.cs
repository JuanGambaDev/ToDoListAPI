using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ToDoListAPI.Models;

public class RefreshToken {

    [Key]
    public string TokenId { get; set; }

    [Required]
    [MaxLength(600)]
    public string Token { get; set; }

    [Required]
    public DateTime ExpiryDate { get; set; }

    [Required]
    public bool Revoked { get; set; } = false;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(User))]
    [Required]    
    public int UserId { get; set; }

    public virtual User User { get; set;}


}