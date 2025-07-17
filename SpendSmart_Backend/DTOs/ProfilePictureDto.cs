using System.ComponentModel.DataAnnotations;

namespace SpendSmart_Backend.DTOs
{
    public class ProfilePictureUploadDto
    {
        [Required(ErrorMessage = "File is required")]
        public IFormFile File { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be greater than 0")]
        public int UserId { get; set; }
    }

    public class UpdateProfilePictureUrlDto
    {
        [Required(ErrorMessage = "User ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be greater than 0")]
        public int UserId { get; set; }

        public string? ProfilePictureUrl { get; set; }

        public string? FileName { get; set; }
    }


    public class ProfilePictureResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? FileName { get; set; }
        public DateTime? UploadedAt { get; set; }
    }

    public class UserProfileDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class DeleteProfilePictureResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
