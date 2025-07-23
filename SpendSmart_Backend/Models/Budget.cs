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

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active, Completed, Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        // Navigation properties
        public virtual ICollection<BudgetCategory> BudgetCategories { get; set; } = new List<BudgetCategory>();
        public virtual ICollection<TransactionBudgetImpact> TransactionBudgetImpacts { get; set; } = new List<TransactionBudgetImpact>();
    }
} 