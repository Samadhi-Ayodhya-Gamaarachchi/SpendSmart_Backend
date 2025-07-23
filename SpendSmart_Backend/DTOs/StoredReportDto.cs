using System.ComponentModel.DataAnnotations;

namespace SpendSmart_Backend.DTOs
{
    public class StoreReportDto
    {
        [Required]
        [MaxLength(100)]
        public string ReportName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Format { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string FirebaseUrl { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        [Required]
        public int UserId { get; set; }

        // Additional metadata fields
        public long? FileSizeBytes { get; set; }

        [MaxLength(100)]
        public string? FileName { get; set; }
    }
}