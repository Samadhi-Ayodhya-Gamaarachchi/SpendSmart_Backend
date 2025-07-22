// DTOs/EnhancedBudgetDTOs.cs - Enhanced versions without validation attributes
using System.ComponentModel.DataAnnotations;

namespace SpendSmart_Backend.DTOs
{
    // Enhanced request DTOs without validation
    public class EnhancedCreateBudgetRequestDto
    {
        public string BudgetName { get; set; }
        public string BudgetType { get; set; }
        public DateTime StartDate { get; set; }
        public string? Description { get; set; }
        public List<BudgetCategoryAllocationDto> CategoryAllocations { get; set; }
    }

    public class EnhancedCreateTransactionRequestDto
    {
        public int CategoryId { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime TransactionDate { get; set; }
        public bool IsRecurring { get; set; } = false;
        public string? RecurringFrequency { get; set; }
        public DateTime? RecurringEndDate { get; set; }
    }

    // Analytics and reporting DTOs
    public class BudgetAnalyticsDto
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
        public int DaysElapsed { get; set; }
        public int TotalDays { get; set; }
        public int DaysRemaining { get; set; }
        public decimal DailyBudgetRate { get; set; }
        public decimal DailySpendingRate { get; set; }
        public decimal ProjectedEndAmount { get; set; }
        public bool IsOnTrack { get; set; }
        public string Status { get; set; }
        public List<CategoryAnalyticsDto> CategoryAnalytics { get; set; } = new List<CategoryAnalyticsDto>();
    }

    public class CategoryAnalyticsDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal AllocatedAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal ProgressPercentage { get; set; }
        public decimal BudgetSharePercentage { get; set; }
        public bool IsOverBudget { get; set; }
        public decimal OverBudgetAmount { get; set; }
        public int TransactionCount { get; set; }
        public decimal AverageTransactionAmount { get; set; }
    }

    public class BudgetComparisonDto
    {
        public string Period { get; set; } // "Current vs Previous Month/Year"
        public BudgetPeriodDto CurrentPeriod { get; set; }
        public BudgetPeriodDto PreviousPeriod { get; set; }
        public decimal SpendingChangePercentage { get; set; }
        public string SpendingTrend { get; set; } // "Increasing", "Decreasing", "Stable"
        public List<CategoryComparisonDto> CategoryComparisons { get; set; } = new List<CategoryComparisonDto>();
    }

    public class BudgetPeriodDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal UtilizationPercentage { get; set; }
    }

    public class CategoryComparisonDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal CurrentSpending { get; set; }
        public decimal PreviousSpending { get; set; }
        public decimal ChangeAmount { get; set; }
        public decimal ChangePercentage { get; set; }
        public string Trend { get; set; }
    }

    // Filter and search DTOs
    public class BudgetFilterDto
    {
        public string? BudgetType { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? EndDateFrom { get; set; }
        public DateTime? EndDateTo { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public List<int>? CategoryIds { get; set; }
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; } = "CreatedAt";
        public string? SortOrder { get; set; } = "desc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class TransactionFilterDto
    {
        public string? TransactionType { get; set; }
        public int? CategoryId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public bool? IsRecurring { get; set; }
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; } = "TransactionDate";
        public string? SortOrder { get; set; } = "desc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    // Bulk operation DTOs
    public class BulkTransactionRequestDto
    {
        public List<EnhancedCreateTransactionRequestDto> Transactions { get; set; }
    }

    public class BulkTransactionResponseDto
    {
        public int TotalTransactions { get; set; }
        public int SuccessfulTransactions { get; set; }
        public int FailedTransactions { get; set; }
        public List<TransactionResponseDto> CreatedTransactions { get; set; } = new List<TransactionResponseDto>();
        public List<BulkOperationErrorDto> Errors { get; set; } = new List<BulkOperationErrorDto>();
    }

    public class BulkOperationErrorDto
    {
        public int Index { get; set; }
        public string Error { get; set; }
        public object? TransactionData { get; set; }
    }
}