using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendSmart_Backend.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Currency { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
        public ICollection<Report> Reports { get; set; }
        public ICollection<Goal> Goals { get; set; }
        public ICollection<Budget> Budgets { get; set; }
        public ICollection<UserAdmin> UserAdmins { get; set; }
        public ICollection<UserAdmin> ManagedUsers { get; set; }


        // Add missing properties for ResetToken and ResetTokenExpiry
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }

        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }



        public ICollection<RecurringTransaction> RecurringTransactions { get; set; }

    }
}
