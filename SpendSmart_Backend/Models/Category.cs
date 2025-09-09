//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace SpendSmart_Backend.Models
//{
//    public class Category
//    {
//        [Key]
//        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//        public int Id { get; set; }
//        public string Name { get; set; }
//        public string Type { get; set; }
//        public ICollection<Transaction> Transactions { get; set; }
//        public ICollection<Budget> Budgets { get; set; }
//        public ICollection<RecurringTransaction> RecurringTransactions { get; set; }
//    }
//}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendSmart_Backend.Models
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Type { get; set; }
        public string Icon { get; set; } = "📦"; // Default icon
        public string Color { get; set; } = "#9E9E9E"; // Default color
        public ICollection<Transaction> Transactions { get; set; }
        public ICollection<Budget> Budgets { get; set; }
        public ICollection<RecurringTransaction> RecurringTransactions { get; set; }

        public virtual ICollection<BudgetCategory> BudgetCategories { get; set; }
        public virtual ICollection<TransactionBudgetImpact> TransactionBudgetImpacts { get; set; }
    }
}

