using System.ComponentModel.DataAnnotations;

namespace SpendSmart_Backend.DTOs
{
    public class CreateRecurringTransactionDto
    {
        [Required]
        [StringLength(20)]
        public string Type { get; set; } // "Income" or "Expense"

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
        public string? Description { get; set; }

        [Required]
        [StringLength(20)]
        public string Frequency { get; set; } // "Daily", "Weekly", "Monthly", "Yearly"

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Occurrences must be at least 1")]
        public int? Occurrences { get; set; }

    }

    //public class UpdateRecurringTransactionDto
    //{
    //    [StringLength(20)]
    //    public string? Type { get; set; }

    //    public int? CategoryId { get; set; }

    //    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    //    public decimal? Amount { get; set; }

    //    [StringLength(20)]
    //    public string? Frequency { get; set; }

    //    public DateTime? StartDate { get; set; }

    //    public DateTime? EndDate { get; set; }

    //    [Range(1, int.MaxValue, ErrorMessage = "Occurrences must be at least 1")]
    //    public int? Occurrences { get; set; }

    //    public bool? AutoDeduction { get; set; }
    //}

    public class RecurringTransactionDto
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string Frequency { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Occurrences { get; set; }
        public int UserId { get; set; }
        public DateTime? NextExecutionDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class RecurringTransactionListDto
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string CategoryName { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string Frequency { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Occurrences { get; set; }
        public DateTime? NextExecutionDate { get; set; }
        public bool IsActive { get; set; }
        public int GeneratedTransactionsCount { get; set; }
    }

    public class RecurringTransactionSummaryDto
    {
        public int TotalActive { get; set; }
        public int TotalInactive { get; set; }
        public decimal MonthlyIncomeAmount { get; set; }
        public decimal MonthlyExpenseAmount { get; set; }
        public List<RecurringTransactionListDto> UpcomingTransactions { get; set; }
    }
}