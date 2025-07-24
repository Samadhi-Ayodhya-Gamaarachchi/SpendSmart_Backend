using System.ComponentModel.DataAnnotations;

namespace SpendSmart_Backend.DTOs
{
    public class BudgetSummaryDto
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal AllocatedAmount { get; set; }
        public decimal SpendAmount { get; set; }
        
        /// <summary>
        /// Remaining amount (AllocatedAmount - SpendAmount)
        /// </summary>
        public decimal RemainingAmount { get; set; }
        
        /// <summary>
        /// Percentage of allocated amount spent (0-100+)
        /// </summary>
        public decimal SpendPercentage { get; set; }
        
        /// <summary>
        /// Percentage of allocated amount remaining (0-100)
        /// </summary>
        public decimal RemainingPercentage { get; set; }
        
        /// <summary>
        /// Status of the budget (Under Budget, Over Budget, On Track, etc.)
        /// </summary>
        public string Status { get; set; } = "Active";
        
        /// <summary>
        /// Month and year of the budget
        /// </summary>
        public DateTime MonthYear { get; set; }
        
        /// <summary>
        /// Indicates if budget is exceeded (spend > allocated)
        /// </summary>
        public bool IsOverBudget { get; set; }
        
        public string? Description { get; set; }
    }
    
    public class BudgetListDto
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal AllocatedAmount { get; set; }
        public decimal SpendAmount { get; set; }
        public decimal SpendPercentage { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime MonthYear { get; set; }
        public bool IsOverBudget { get; set; }
    }
    
    public class BudgetOverviewDto
    {
        public DateTime MonthYear { get; set; }
        public decimal TotalAllocated { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal TotalRemaining { get; set; }
        public decimal OverallSpendPercentage { get; set; }
        public int TotalBudgets { get; set; }
        public int OverBudgetCount { get; set; }
        public List<BudgetListDto> BudgetDetails { get; set; } = new List<BudgetListDto>();
    }
    
    public class CreateBudgetDto
    {
        [Required]
        public DateTime MonthYear { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Allocated amount must be greater than 0")]
        public decimal AllocatedAmount { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Spend amount cannot be negative")]
        public decimal SpendAmount { get; set; } = 0;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Required]
        public int CategoryId { get; set; }
    }
    
    public class UpdateBudgetDto
    {
        public DateTime? MonthYear { get; set; }
        
        [Range(0.01, double.MaxValue, ErrorMessage = "Allocated amount must be greater than 0")]
        public decimal? AllocatedAmount { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Spend amount cannot be negative")]
        public decimal? SpendAmount { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public int? CategoryId { get; set; }
    }
    
    public class AddExpenseToBudgetDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
        
        [StringLength(200)]
        public string? Note { get; set; }
    }
}