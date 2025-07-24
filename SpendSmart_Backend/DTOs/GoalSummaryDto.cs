using System.ComponentModel.DataAnnotations;

namespace SpendSmart_Backend.DTOs
{
    public class GoalSummaryDto
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public DateTime EndDate { get; set; }
        
        public decimal CurrentAmount { get; set; }
        
        public decimal TargetAmount { get; set; }
        
        /// <summary>
        /// Percentage of target amount achieved (0-100)
        /// </summary>
        public decimal SavedPercentage { get; set; }
        
        /// <summary>
        /// Remaining amount to reach the target
        /// </summary>
        public decimal RemainingAmount { get; set; }
        
        /// <summary>
        /// Number of days remaining until end date
        /// </summary>
        public int DaysRemaining { get; set; }
        
        /// <summary>
        /// Indicates if the goal is achieved (100% or more)
        /// </summary>
        public bool IsAchieved { get; set; }
        
        /// <summary>
        /// Status of the goal (Active, Achieved, Overdue, etc.)
        /// </summary>
        public string Status { get; set; } = "Active";
    }
    
    public class GoalListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime EndDate { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal SavedPercentage { get; set; }
        public string Status { get; set; } = "Active";
        public int DaysRemaining { get; set; }
    }
    
    public class CreateGoalDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
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

    public class SavingsSummaryDto
    {
        /// <summary>
        /// Total current amount saved across all active goals
        /// </summary>
        public decimal TotalSavings { get; set; }
        
        /// <summary>
        /// Total target amount across all active goals
        /// </summary>
        public decimal TotalTargetAmount { get; set; }
        
        /// <summary>
        /// Overall savings percentage across all active goals
        /// </summary>
        public decimal OverallSavingsPercentage { get; set; }
        
        /// <summary>
        /// Number of active goals
        /// </summary>
        public int ActiveGoalsCount { get; set; }
        
        /// <summary>
        /// Total remaining amount to reach all targets
        /// </summary>
        public decimal TotalRemainingAmount { get; set; }
        
        /// <summary>
        /// List of active goals contributing to savings
        /// </summary>
        public List<GoalListDto> ActiveGoals { get; set; } = new List<GoalListDto>();
    }
}