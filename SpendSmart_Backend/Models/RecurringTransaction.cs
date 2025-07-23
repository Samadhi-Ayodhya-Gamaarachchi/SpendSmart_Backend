using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendSmart_Backend.Models
{
    public class RecurringTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Type { get; set; }
        public int CategoryId { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; } // <-- Add this line
        public string Frequency { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Occurrences { get; set; }
        public bool AutoDeduction { get; set; }
        public int UserId { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        public ICollection<Transaction> Transactions { get; set; }
    }
}
