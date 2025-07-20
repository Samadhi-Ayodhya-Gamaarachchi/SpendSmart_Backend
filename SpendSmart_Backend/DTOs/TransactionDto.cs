using System.ComponentModel.DataAnnotations;

namespace SpendSmart_Backend.DTOs
{
    public class CreateTransactionRequestDto
    {
        [Required]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(20)]
        public string TransactionType { get; set; } // Income, Expense

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; }

        public bool IsRecurring { get; set; } = false;

        [MaxLength(20)]
        public string? RecurringFrequency { get; set; }

        public DateTime? RecurringEndDate { get; set; }
    }

    public class TransactionResponseDto
    {
        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime TransactionDate { get; set; }
        public bool IsRecurring { get; set; }
        public string? RecurringFrequency { get; set; }
        public DateTime? RecurringEndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<BudgetImpactDto> BudgetImpacts { get; set; } = new List<BudgetImpactDto>();
    }

    public class BudgetImpactDto
    {
        public int BudgetId { get; set; }
        public string BudgetName { get; set; }
        public decimal ImpactAmount { get; set; }
    }
}