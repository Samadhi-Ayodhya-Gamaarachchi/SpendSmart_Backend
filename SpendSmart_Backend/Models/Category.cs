using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendSmart_Backend.Models
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Type { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
        public ICollection<Budget> Budgets { get; set; }
        public ICollection<RecurringTransaction> RecurringTransactions { get; set; }

    }
}

