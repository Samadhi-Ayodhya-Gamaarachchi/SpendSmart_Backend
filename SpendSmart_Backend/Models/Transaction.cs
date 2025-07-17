using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendSmart_Backend.Models
{
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Type { get; set; }
        public int CategoryId { get; set; }
        public decimal Amount { get; set; }

        public int? BudgetCategoryId { get; set; } // Nullable to allow transactions not linked to a budget category
        [ForeignKey("BudgetCategoryId")]
        public BudgetCategory? BudgetCategory { get; set; } // Nullable to allow transactions not linked to a budget category
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public int UserId { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

    }
}
