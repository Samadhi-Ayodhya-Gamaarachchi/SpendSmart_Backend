using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendSmart_Backend.Models
{
    public class Goal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Description { get; set; }

        // Foreign key for User
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        // Navigation property for related SavingRecords
        public ICollection<SavingRecord> SavingRecords { get; set; } = new List<SavingRecord>();
    }
}