using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendSmart_Backend.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; }
        public string Currency { get; set; }

        

        public ICollection<Transaction> Transactions { get; set; }
        public ICollection<Report> Reports { get; set; }
        public ICollection<Goal> Goals { get; set; }
        public ICollection<Budget> Budgets { get; set; }
        public ICollection<UserAdmin> UserAdmins { get; set; }
        public ICollection<UserAdmin> ManagedUsers { get; set; }
    }
}
