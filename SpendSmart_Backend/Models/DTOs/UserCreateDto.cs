using System.ComponentModel.DataAnnotations;

namespace SpendSmart_Backend.Models.DTOs
{
    public class UserCreateDto
    {
        [Required]
        [StringLength(50)]
        public required string UserName { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public required string Password { get; set; }

        [StringLength(50)]
        public string? FirstName { get; set; }

        [StringLength(50)]
        public string? LastName { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [StringLength(10)]
        public required string Currency { get; set; }
    }
}
