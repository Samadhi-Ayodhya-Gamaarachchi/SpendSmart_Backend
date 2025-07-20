using System.ComponentModel.DataAnnotations;

namespace SpendSmart_Backend.DTOs
{
    public class EmailChangeRequestDto
    {
        [Required]
        [EmailAddress]
        public string NewEmail { get; set; } = string.Empty;

        // Temporary: for testing without JWT authentication
        public int UserId { get; set; }
    }
}
