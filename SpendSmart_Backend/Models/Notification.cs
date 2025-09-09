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
        public NotificationType Type { get; set; } // Use enum instead of string

        [Required]
        [StringLength(200)]
        public required string Title { get; set; }

        [Required]
        [StringLength(500)]
        public required string Message { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } // Remove default value, set in DbContext

        [Required]
        public NotificationPriority Priority { get; set; } = NotificationPriority.Medium; // Use enum

        // Optional - for user-specific alerts
        public int? RelatedUserId { get; set; }
        
        [StringLength(100)]
        public string? RelatedUserName { get; set; }

        // Navigation property
        [ForeignKey("RelatedUserId")]
        public User? RelatedUser { get; set; }

        // Auto-delete after 30 days
        public DateTime ExpiresAt { get; set; } // Remove default value, set in DbContext
    }

    // Enum for notification types
    public enum NotificationType
    {
        NewUser,
        InactiveUser,
        EmailServiceFailure,
        BudgetExceeded,
        RecurringTransactionCreated,
        SystemAlert
    }

    // Enum for priority levels
    public enum NotificationPriority
    {
        Low,
        Medium,
        High,
        Critical
    }
}
