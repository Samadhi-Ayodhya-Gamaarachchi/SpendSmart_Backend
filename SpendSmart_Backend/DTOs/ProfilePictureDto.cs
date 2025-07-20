using System.ComponentModel.DataAnnotations;

namespace SpendSmart_Backend.DTOs
{
    public class ProfilePictureUploadDto
    {
        [Required(ErrorMessage = "File is required")]
        public IFormFile File { get; set; } = null!;

        [Required(ErrorMessage = "User ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be greater than 0")]
        public int UserId { get; set; }
    }

    public class UpdateProfilePictureUrlDto
    {
        [Required(ErrorMessage = "User ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be greater than 0")]
        public int UserId { get; set; }

        [Url(ErrorMessage = "Invalid URL format")]
        public string? ProfilePictureUrl { get; set; }

        [StringLength(255, ErrorMessage = "File name cannot exceed 255 characters")]
        public string? FileName { get; set; }
    }


    public class ProfilePictureResponseDto
    {
        public bool Success { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        public string? ProfilePictureUrl { get; set; }
        public string? FileName { get; set; }
        public DateTime? UploadedAt { get; set; }
    }

    public class UserProfileDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Url(ErrorMessage = "Invalid URL format")]
        public string? ProfilePictureUrl { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class DeleteProfilePictureResponseDto
    {
        public bool Success { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        public DateTime? DeletedAt { get; set; }
    }
}
