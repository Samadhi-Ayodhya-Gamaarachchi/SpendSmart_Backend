using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendSmart_Backend.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(20)]
        public string TransactionType { get; set; } // Income, Expense

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(100)]
        public string? MerchantName { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        [MaxLength(500)]
        public string? Tags { get; set; } // Comma-separated list of tags

        [MaxLength(1000)]
        public string? ReceiptUrl { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime TransactionDate { get; set; }

        public bool IsRecurring { get; set; } = false;

        [MaxLength(20)]
        public string? RecurringFrequency { get; set; } // Daily, Weekly, Monthly, Annually

        [Column(TypeName = "date")]
        public DateTime? RecurringEndDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        // Navigation properties
        public virtual ICollection<TransactionBudgetImpact> TransactionBudgetImpacts { get; set; } = new List<TransactionBudgetImpact>();
    }
}
