
// Models/BudgetCategory.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendSmart_Backend.Models
{
    public class BudgetCategory
    {
        [Key]
        public int BudgetCategoryId { get; set; }

        [Required]
        public int BudgetId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AllocatedAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SpentAmount { get; set; } = 0;

        [NotMapped]
        public decimal RemainingAmount => AllocatedAmount - SpentAmount;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        [ForeignKey("BudgetId")]
        public virtual Budget Budget { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }
    }
}