using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendSmart_Backend.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        [Required]
        public string? FirstName { get; set; }
        [Required]
        public string? LastName { get; set; }
        public string Email { get; set; }

        public string? ProfilePictureUrl { get; set; }
        public string? ProfilePicturePath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;
        public string Status { get; set; } = "Active";

        public ICollection<Transaction> Transactions { get; set; }
        public ICollection<Report> Reports { get; set; }
        public ICollection<Goal> Goals { get; set; }
        public ICollection<Budget> Budgets { get; set; }
        public ICollection<UserAdmin> UserAdmins { get; set; }
        public ICollection<UserAdmin> ManagedUsers { get; set; }

        public ICollection<SavingRecord> SavingRecords { get; set; }



        // Add missing properties for ResetToken and ResetTokenExpiry
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }

        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }



        public ICollection<RecurringTransaction> RecurringTransactions { get; set; }

        // Add missing properties for ResetToken and ResetTokenExpiry
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }

        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }

        // Email change tracking
        public string? PendingEmail { get; set; }
        public string? EmailChangeToken { get; set; }
        public DateTime? EmailChangeTokenExpiry { get; set; }


    }
}
