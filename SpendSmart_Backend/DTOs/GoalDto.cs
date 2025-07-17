using System.ComponentModel.DataAnnotations;

namespace SpendSmart_Backend.DTOs
{
    public class GoalDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Description { get; set; }
        public int UserId { get; set; }
        public decimal Progress { get; set; }
        public int? RemainingDays { get; set; }
    }

    public class CreateGoalDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Target amount must be greater than 0")]
        public decimal TargetAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Current amount cannot be negative")]
        public decimal CurrentAmount { get; set; } = 0;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public int UserId { get; set; }
    }

    public class UpdateGoalDto
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Target amount must be greater than 0")]
        public decimal? TargetAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Current amount cannot be negative")]
        public decimal? CurrentAmount { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
    }
}
