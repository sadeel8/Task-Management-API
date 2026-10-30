using System.ComponentModel.DataAnnotations;

namespace Task_Management_API.Models
{
    public class User
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public required string Username { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string HashedPassword { get; set; }
    }
}
