using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendSmart_Backend.Models
{
    public class Budget
    {
        [Key]
        public int BudgetId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string BudgetName { get; set; }

        [Required]
        [MaxLength(20)]
        public string BudgetType { get; set; } // Monthly, Annually

        [Required]
        [Column(TypeName = "date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime EndDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalBudgetAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalSpentAmount { get; set; } = 0;


        [NotMapped]
        public decimal RemainingAmount => TotalBudgetAmount - TotalSpentAmount;

        [NotMapped]
        public decimal ProgressPercentage => TotalBudgetAmount > 0 ? (TotalSpentAmount / TotalBudgetAmount) * 100 : 0;

        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active, Completed, Cancelled

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Helper method to calculate end date
        public static DateTime CalculateEndDate(DateTime startDate, string budgetType)
        {
            return budgetType.ToLower() switch
            {
                "monthly" => startDate.AddMonths(1).AddDays(-1),
                "annually" => startDate.AddYears(1).AddDays(-1),
                _ => startDate.AddMonths(1).AddDays(-1)
            };
        }

        // Method to check if a date falls within budget period
        public bool IsDateInBudgetPeriod(DateTime date)
        {
            return date.Date >= StartDate.Date && date.Date <= EndDate.Date;
        }


        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        // Navigation properties
        public virtual ICollection<BudgetCategory> BudgetCategories { get; set; } = new List<BudgetCategory>();
        public virtual ICollection<TransactionBudgetImpact> TransactionBudgetImpacts { get; set; } = new List<TransactionBudgetImpact>();
    }
}