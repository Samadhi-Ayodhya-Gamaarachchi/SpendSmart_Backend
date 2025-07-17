using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SpendSmart_Backend.Models
{
    public class BudgetCategory

    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int BudgetId{ get; set; }
        public int CategoryId { get; set; }
        public decimal AllocatedAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public int UserId { get; set; }

        [ForeignKey("BudgetId")]
        public Budget Budget { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        
    }
}

////CREATE TABLE BudgetCategories (
//Id INT PRIMARY KEY,                -- Unique identifier for each budget-category entry
//    BudgetId INT NOT NULL,             -- Foreign key linking to the Budgets table
//    CategoryId INT NOT NULL,           -- Foreign key linking to the Categories table
//    AllocatedAmount DECIMAL(10, 2) NOT NULL, -- Total allocated amount for the category
//    RemainingAmount DECIMAL(10, 2) NOT NULL, -- Remaining amount after deducting expenses
//    FOREIGN KEY (BudgetId) REFERENCES Budgets(Id), -- Ensure valid budget association
//    FOREIGN KEY (CategoryId) REFERENCES Categories(Id) -- Ensure valid category association
//);