using System.ComponentModel.DataAnnotations;

namespace SpendSmart_Backend.Models.DTOs
{
    public class AdminCreateDto
    {
        [Required]
        public required string Name { get; set; }
        
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        
        [Required]
        public required string Password { get; set; }
    }
}
