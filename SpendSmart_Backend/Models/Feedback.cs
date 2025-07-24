using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourApp.Models
{
    public class Feedback
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [StringLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters")]
        public string? Comment { get; set; }

        [StringLength(100)]
        public string? PageContext { get; set; } // e.g., "user-settings", "dashboard", etc.

        [Required]
        public DateTime SubmittedAt { get; set; }

        public bool IsProcessed { get; set; } = false;

        public DateTime? ProcessedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; } // Adjust based on your User model name

        // CUSTOMIZE: Add more fields if needed
        // public string? UserAgent { get; set; } // Browser info
        // public string? IpAddress { get; set; } // User IP
        // public string? SessionId { get; set; } // Session tracking
        // public int? ParentFeedbackId { get; set; } // For replies/follow-ups
        // public string? Category { get; set; } // Feedback category
        // public int? Priority { get; set; } // Priority level
        // public string? Status { get; set; } // "new", "in-progress", "resolved"
        // public string? AdminResponse { get; set; } // Admin response to feedback
        // public DateTime? ResponseSentAt { get; set; }
    }
}