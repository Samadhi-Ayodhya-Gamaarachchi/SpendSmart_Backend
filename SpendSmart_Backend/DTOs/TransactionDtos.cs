using System;
using System.Collections.Generic;

namespace SpendSmart_Backend.DTOs
{
    // DTO for creating or updating a transaction
    public class CreateTransactionDto
    {
        public required string TransactionType { get; set; } // "Income" or "Expense"
        public required int CategoryId { get; set; }
        public required decimal Amount { get; set; }
        public required string TransactionDate { get; set; } // ISO date string
        public string? Description { get; set; }
        public string? MerchantName { get; set; }
        public string? Location { get; set; }
        public string[]? Tags { get; set; }
        public string? ReceiptUrl { get; set; }
        public bool IsRecurring { get; set; }
        public string? RecurringFrequency { get; set; } // "Daily", "Weekly", "Monthly", "Annually"
        public string? RecurringEndDate { get; set; } // ISO date string
    }

    // DTO for transaction list view
    public class TransactionViewDto
    {
        public int Id { get; set; }
        public required string Type { get; set; }
        public required string Category { get; set; }
        public decimal Amount { get; set; }
        public required string Date { get; set; }
        public string? Description { get; set; }
        public int UserId { get; set; }
    }

    // DTO for detailed transaction view
    public class TransactionDetailsDto
    {
        public int TransactionId { get; set; }
        public required string TransactionType { get; set; }
        public int CategoryId { get; set; }
        public required string CategoryName { get; set; }
        public decimal Amount { get; set; }
        public required string TransactionDate { get; set; }
        public string? Description { get; set; }
        public string? MerchantName { get; set; }
        public string? Location { get; set; }
        public string[]? Tags { get; set; }
        public string? ReceiptUrl { get; set; }
        public bool IsRecurring { get; set; }
        public string? RecurringFrequency { get; set; }
        public string? RecurringEndDate { get; set; }
        public List<BudgetImpactDto> BudgetImpacts { get; set; } = new List<BudgetImpactDto>();
    }

    // DTO for budget impact information
    public class BudgetImpactDto
    {
        public int BudgetId { get; set; }
        public required string BudgetName { get; set; }
        public int CategoryId { get; set; }
        public required string CategoryName { get; set; }
        public decimal ImpactAmount { get; set; }
        public required string ImpactType { get; set; } // "Deduction" for expenses
    }

    // DTO for transaction statistics
    public class TransactionStatsDto
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetAmount { get; set; }
        public List<CategorySummaryDto> TopCategories { get; set; } = new List<CategorySummaryDto>();
        public List<MonthlyTotalDto> MonthlyTotals { get; set; } = new List<MonthlyTotalDto>();
    }

    // DTO for category summary in statistics
    public class CategorySummaryDto
    {
        public int CategoryId { get; set; }
        public required string CategoryName { get; set; }
        public string? CategoryIcon { get; set; }
        public string? CategoryColor { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Percentage { get; set; }
    }

    // DTO for monthly totals in statistics
    public class MonthlyTotalDto
    {
        public required string Month { get; set; } // Format: "YYYY-MM"
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
        public decimal NetAmount { get; set; }
    }
} 