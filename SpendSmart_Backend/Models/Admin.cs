using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendSmart_Backend.Models
{
    public class Admin
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        
        // Profile Picture Properties
        public string? ProfilePicture { get; set; } // Base64 encoded image
        public string? ProfilePictureFileName { get; set; } // Original file name
        public DateTime? ProfilePictureUploadedAt { get; set; } // Upload timestamp

        // Email Verification Properties
        public string? PendingEmail { get; set; } // Email waiting for verification
        public EmailVerificationStatus EmailStatus { get; set; } = EmailVerificationStatus.Verified;

        public ICollection<UserAdmin> UserAdmins { get; set; } = new List<UserAdmin>();
        public ICollection<EmailVerification> EmailVerifications { get; set; } = new List<EmailVerification>();
    }
}
