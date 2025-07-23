using System.ComponentModel.DataAnnotations;

namespace SpendSmart_Backend.DTOs
{
    public class CreateSavingRecordDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public string? Description { get; set; }

        [Required]
        public int GoalId { get; set; }

        public int? UserId { get; set; }
    }
}