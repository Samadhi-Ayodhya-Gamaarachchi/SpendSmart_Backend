using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpendSmart_Backend.domain.Entities
{
    public class Budget
    {
        [Key]
        public int BudgetId { get; set; }
        public string MonthYear { get; set; }
        public decimal AllocatedAmount { get; set; }
        public decimal SpendAmount { get; set; }
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}
