using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendSmart_Backend.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public required string Email { get; set; }
        public required string Currency { get; set; }
        
        // Activity Tracking Fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;
        public string Status { get; set; } = "Active"; // Active, Inactive, Suspended
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Report> Reports { get; set; } = new List<Report>();
        public ICollection<Goal> Goals { get; set; } = new List<Goal>();
        public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
        public ICollection<UserAdmin> UserAdmins { get; set; } = new List<UserAdmin>();
        public ICollection<UserAdmin> ManagedUsers { get; set; } = new List<UserAdmin>();
    }
}
