using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendSmart_Backend.Models
{
    public class EmailVerification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        public int AdminId { get; set; }
        
        [Required]
        [EmailAddress]
        public string NewEmail { get; set; } = string.Empty;
        
        [Required]
        public string VerificationToken { get; set; } = string.Empty;
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(24);
        
        public bool IsVerified { get; set; } = false;
        
        public DateTime? VerifiedAt { get; set; }
        
        // Navigation property
        [ForeignKey("AdminId")]
        public Admin Admin { get; set; } = null!;
    }
}
