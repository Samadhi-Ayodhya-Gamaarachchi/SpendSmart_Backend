namespace SpendSmart_Backend.Models.DTOs
{
    // DTO for admin profile responses (includes profile picture data)
    public class AdminResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        
        // Profile Picture Properties
        public string? ProfilePictureUrl { get; set; } // Base64 data URL or endpoint URL
        public string? ProfilePictureFileName { get; set; }
        public DateTime? ProfilePictureUploadedAt { get; set; }
        
        // Computed property to check if admin has profile picture
        public bool HasProfilePicture => !string.IsNullOrEmpty(ProfilePictureUrl);
    }
}
