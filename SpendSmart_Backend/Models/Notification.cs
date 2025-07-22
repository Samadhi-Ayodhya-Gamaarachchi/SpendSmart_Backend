using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendSmart_Backend.Models
{
    public class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public required string Type { get; set; } // NewUser, InactiveUser, EmailServiceFailure

        [Required]
        [StringLength(200)]
        public required string Title { get; set; }

        [Required]
        [StringLength(500)]
        public required string Message { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(20)]
        public required string Priority { get; set; } = "Medium"; // Low, Medium, High

        // Optional - for user-specific alerts
        public int? RelatedUserId { get; set; }
        public string? RelatedUserName { get; set; }

        // Auto-delete after 30 days
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(30);
    }

    // Enum for notification types
    public enum NotificationType
    {
        NewUser,
        InactiveUser, 
        EmailServiceFailure
    }

    // Enum for priority levels
    public enum NotificationPriority
    {
        Low,
        Medium,
        High
    }
}
