using System.ComponentModel.DataAnnotations;

namespace SpendSmart_Backend.Models.DTOs
{
    public class AdminProfileUpdateDto
    {
        [Required]
        public required string Name { get; set; }
        
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        
        // Current password for verification (required when changing password or email)
        public string? CurrentPassword { get; set; }
        
        // Password is optional during update
        public string? Password { get; set; }
    }
}
