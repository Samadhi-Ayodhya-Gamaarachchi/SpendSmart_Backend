using System.ComponentModel.DataAnnotations;

namespace SpendSmart_Backend.Models.DTOs
{
    public class UserUpdateDto
    {
        [StringLength(50)]
        public string? UserName { get; set; }

        [StringLength(100, MinimumLength = 6)]
        public string? Password { get; set; }

        [StringLength(50)]
        public string? FirstName { get; set; }

        [StringLength(50)]
        public string? LastName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(10)]
        public string? Currency { get; set; }

        public bool? IsActive { get; set; }

        public string? Status { get; set; }

        // For password changes, require current password
        public string? CurrentPassword { get; set; }
    }
}
