using System.ComponentModel.DataAnnotations;

namespace SpendSmart_Backend.DTOs
{
    // Request DTOs
    public class CreateBudgetRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string BudgetName { get; set; }

        [Required]
        [MaxLength(20)]
        public string BudgetType { get; set; } // Monthly, Annually

        [Required]
        public DateTime StartDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public List<BudgetCategoryAllocationDto> CategoryAllocations { get; set; }
    }

    public class BudgetCategoryAllocationDto
    {
        [Required]
        public int CategoryId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Allocated amount must be greater than 0")]
        public decimal AllocatedAmount { get; set; }
    }

    public class UpdateBudgetRequestDto
    {
        [MaxLength(100)]
        public string? BudgetName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(20)]
        public string? Status { get; set; } // Active, Completed, Cancelled

        public List<BudgetCategoryAllocationDto>? CategoryAllocations { get; set; }
    }

    // Response DTOs
    public class BudgetResponseDto
    {
        public int BudgetId { get; set; }
        public int UserId { get; set; }
        public string BudgetName { get; set; }
        public string BudgetType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalBudgetAmount { get; set; }
        public decimal TotalSpentAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal ProgressPercentage { get; set; }
        public string Status { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<BudgetCategoryResponseDto> Categories { get; set; } = new List<BudgetCategoryResponseDto>();
    }

    public class BudgetCategoryResponseDto
    {
        public int BudgetCategoryId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal AllocatedAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal ProgressPercentage => AllocatedAmount > 0 ? (SpentAmount / AllocatedAmount) * 100 : 0;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class BudgetSummaryDto
    {
        public int BudgetId { get; set; }
        public string BudgetName { get; set; }
        public string BudgetType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalBudgetAmount { get; set; }
        public decimal TotalSpentAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal ProgressPercentage { get; set; }
        public string Status { get; set; }
        public int DaysRemaining => (EndDate - DateTime.UtcNow.Date).Days;
        public bool IsOverBudget => TotalSpentAmount > TotalBudgetAmount;
    }
}
