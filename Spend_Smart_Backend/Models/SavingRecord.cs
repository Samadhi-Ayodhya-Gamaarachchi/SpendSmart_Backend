using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SpendSmart_Backend.Models
{
    public class SavingRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string? Description { get; set; }
        // Foreign key for User
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        // Foreign key for Goal
        public int GoalId { get; set; }
        [ForeignKey("GoalId")]
        public Goal Goal { get; set; } = null!;
    }
}