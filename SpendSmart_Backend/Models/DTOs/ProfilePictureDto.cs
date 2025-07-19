using System.ComponentModel.DataAnnotations;

namespace SpendSmart_Backend.Models.DTOs
{
    // DTO for profile picture upload requests
    public class ProfilePictureUploadDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;
    }

    // DTO for profile picture response
    public class ProfilePictureResponseDto
    {
        public string? Base64Image { get; set; }
        public string? FileName { get; set; }
        public DateTime? UploadedAt { get; set; }
    }

    // DTO for profile picture update (same as upload)
    public class ProfilePictureUpdateDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;
    }
}
