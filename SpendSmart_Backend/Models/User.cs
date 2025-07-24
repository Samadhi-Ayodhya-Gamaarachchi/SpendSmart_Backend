using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendSmart_Backend.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string? FirstName { get; set; }
        [Required]
        public string? LastName { get; set; }
        [Required]
        public string Email { get; set; }


        //profile picture settings
        public string? ProfilePictureUrl { get; set; }
        public string? ProfilePicturePath { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }


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

        // Email change tracking
        public string? PendingEmail { get; set; }
        public string? EmailChangeToken { get; set; }
        public DateTime? EmailChangeTokenExpiry { get; set; }

    }
}
