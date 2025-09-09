using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendSmart_Backend.Models
{
    public class Report
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string ReportName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Format { get; set; } = string.Empty;

        [Required]
        public DateTime DateGenerated { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [MaxLength(500)]
        public string FirebaseUrl { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        [Required]
        public int UserId { get; set; }

        // Additional metadata columns for better tracking
        public long? FileSizeBytes { get; set; }

        [MaxLength(100)]
        public string? FileName { get; set; }

        public DateTime? LastAccessed { get; set; }

        public int AccessCount { get; set; } = 0;

        [MaxLength(50)]
        public string Status { get; set; } = "Active"; // Active, Archived, Deleted

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}