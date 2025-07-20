using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpendSmart_Backend.domain.Entities
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string CategoryName { get; set; }
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public int UserId { get; set; }

        [ForeignKey("CategoryName")]
        public Category Category { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        
    }
}
