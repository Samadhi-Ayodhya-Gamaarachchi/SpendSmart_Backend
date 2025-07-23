using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendSmart_Backend.Models
{
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Type { get; set; } // Income, Expense

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }        

        [Required]
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        // Foreign keys
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        [ForeignKey("RecurringTransactionId")]
        public RecurringTransaction? RecurringTransaction { get; set; }

        // Navigation properties
        public virtual ICollection<TransactionBudgetImpact> TransactionBudgetImpacts { get; set; } = new List<TransactionBudgetImpact>();
    }
}
