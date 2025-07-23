namespace SpendSmart_Backend.DTOs
{
    // Request DTOs
    public class CategoryAllocationDto
    {
        public int CategoryId { get; set; }
        public decimal AllocatedAmount { get; set; }
    }

    public class CreateBudgetDto
    {
        public string BudgetName { get; set; } = string.Empty;
        public string BudgetType { get; set; } = string.Empty; // Monthly, Annually
        public DateTime StartDate { get; set; }
        public string? Description { get; set; }
        public List<CategoryAllocationDto> CategoryAllocations { get; set; } = new List<CategoryAllocationDto>();
    }

    public class UpdateBudgetDto
    {
        public string BudgetName { get; set; } = string.Empty;
        public string BudgetType { get; set; } = string.Empty; // Monthly, Annually
        public DateTime StartDate { get; set; }
        public string? Description { get; set; }
        public List<CategoryAllocationDto> CategoryAllocations { get; set; } = new List<CategoryAllocationDto>();
    }

    // Response DTOs
    public class BudgetCategoryResponseDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryIcon { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = string.Empty;
        public decimal AllocatedAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal ProgressPercentage { get; set; }
    }

    public class BudgetResponseDto
    {
        public int BudgetId { get; set; }
        public string BudgetName { get; set; } = string.Empty;
        public string BudgetType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalBudgetAmount { get; set; }
        public decimal TotalSpentAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string? Description { get; set; }
        public decimal ProgressPercentage { get; set; }
        public int DaysRemaining { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<BudgetCategoryResponseDto> Categories { get; set; } = new List<BudgetCategoryResponseDto>();
    }

    public class BudgetSummaryDto
    {
        public int BudgetId { get; set; }
        public string BudgetName { get; set; } = string.Empty;
        public string BudgetType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalBudgetAmount { get; set; }
        public decimal TotalSpentAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal ProgressPercentage { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ExpenseBreakdownDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }

    public class PeriodDataDto
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public decimal CumulativeAmount { get; set; }
        public decimal BudgetLimit { get; set; }
    }
} 