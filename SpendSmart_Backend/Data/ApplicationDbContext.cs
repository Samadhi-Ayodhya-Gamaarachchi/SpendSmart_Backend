using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Models;

namespace SpendSmart_Backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure UserAdmin composite key
            modelBuilder.Entity<UserAdmin>()
                .HasKey(ua => new { ua.UserId, ua.ManagerId });

            modelBuilder.Entity<UserAdmin>()
                .HasOne(ua => ua.User)
                .WithMany(u => u.UserAdmins)
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserAdmin>()
                .HasOne(ua => ua.Manager)
                .WithMany(u => u.ManagedUsers)
                .HasForeignKey(ua => ua.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Configure Goal entity
            modelBuilder.Entity<Goal>(entity =>
            {
                entity.Property(e => e.TargetAmount).HasPrecision(18, 2);
                entity.Property(e => e.CurrentAmount).HasPrecision(18, 2);

                entity.HasOne(g => g.User)
                    .WithMany(u => u.Goals)
                    .HasForeignKey(g => g.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure SavingRecord entity
            



            modelBuilder.Entity<Transaction>()
               .HasOne(t => t.RecurringTransaction)
               .WithMany(rt => rt.Transactions)
               .HasForeignKey(t => t.RecurringTransactionId)
               .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Transaction>()
              .HasOne(t => t.User)
              .WithMany(u => u.Transactions)
              .HasForeignKey(t => t.UserId)
              .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
              .HasOne(t => t.Category)
              .WithMany(c => c.Transactions)
              .HasForeignKey(t => t.CategoryId)
              .OnDelete(DeleteBehavior.Restrict);

        }


        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<RecurringTransaction> RecurringTransactions { get; set; }
        public DbSet<Budget> Budgets { get; set; }

        public DbSet<BudgetCategory> BudgetCategories { get; set; }
        public DbSet<TransactionBudgetImpact> TransactionBudgetImpacts { get; set; }
        public DbSet<SavingRecord> SavingRecords { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<EmailVerification> EmailVerifications { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.UserName).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Admin entity configuration
            modelBuilder.Entity<Admin>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.UserName).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Category entity configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasOne(c => c.User)
                      .WithMany(u => u.Categories)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Transaction entity configuration
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasOne(t => t.User)
                      .WithMany(u => u.Transactions)
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(t => t.Category)
                      .WithMany(c => c.Transactions)
                      .HasForeignKey(t => t.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.RecurringTransaction)
                      .WithMany(rt => rt.Transactions)
                      .HasForeignKey(t => t.RecurringTransactionId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.Property(t => t.Amount).HasPrecision(18, 2);
            });

            // RecurringTransaction entity configuration
            modelBuilder.Entity<RecurringTransaction>(entity =>
            {
                entity.HasOne(rt => rt.User)
                      .WithMany(u => u.RecurringTransactions)
                      .HasForeignKey(rt => rt.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rt => rt.Category)
                      .WithMany(c => c.RecurringTransactions)
                      .HasForeignKey(rt => rt.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(rt => rt.Amount).HasPrecision(18, 2);
            });

            // Budget entity configuration
            modelBuilder.Entity<Budget>(entity =>
            {
                entity.HasOne(b => b.User)
                      .WithMany(u => u.Budgets)
                      .HasForeignKey(b => b.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(b => b.TotalAmount).HasPrecision(18, 2);
                entity.Property(b => b.SpentAmount).HasPrecision(18, 2);
                entity.Property(b => b.RemainingAmount).HasPrecision(18, 2);
            });

            // BudgetCategory entity configuration
            modelBuilder.Entity<BudgetCategory>(entity =>
            {
                entity.HasOne(bc => bc.Budget)
                      .WithMany(b => b.BudgetCategories)
                      .HasForeignKey(bc => bc.BudgetId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(bc => bc.Category)
                      .WithMany(c => c.BudgetCategories)
                      .HasForeignKey(bc => bc.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(bc => bc.AllocatedAmount).HasPrecision(18, 2);
                entity.Property(bc => bc.SpentAmount).HasPrecision(18, 2);
            });

            // TransactionBudgetImpact entity configuration
            modelBuilder.Entity<TransactionBudgetImpact>(entity =>
            {
                entity.HasOne(tbi => tbi.Transaction)
                      .WithMany(t => t.TransactionBudgetImpacts)
                      .HasForeignKey(tbi => tbi.TransactionId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(tbi => tbi.Budget)
                      .WithMany(b => b.TransactionBudgetImpacts)
                      .HasForeignKey(tbi => tbi.BudgetId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(tbi => tbi.ImpactAmount).HasPrecision(18, 2);
            });

            // SavingRecord entity configuration
            modelBuilder.Entity<SavingRecord>(entity =>
            {
                entity.HasOne(sr => sr.User)
                      .WithMany(u => u.SavingRecords)
                      .HasForeignKey(sr => sr.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(sr => sr.Amount).HasPrecision(18, 2);
            });

            // Report entity configuration
            modelBuilder.Entity<Report>(entity =>
            {
                entity.HasOne(r => r.User)
                      .WithMany(u => u.Reports)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(r => r.GeneratedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // EmailVerification entity configuration
            modelBuilder.Entity<EmailVerification>(entity =>
            {
                entity.HasOne(ev => ev.User)
                      .WithMany(u => u.EmailVerifications)
                      .HasForeignKey(ev => ev.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(ev => ev.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(ev => ev.ExpiresAt).HasDefaultValueSql("DATEADD(hour, 24, GETUTCDATE())");
            });

            // Notification entity configuration
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasOne(n => n.RelatedUser)
                      .WithMany(u => u.Notifications)
                      .HasForeignKey(n => n.RelatedUserId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.Property(n => n.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(n => n.ExpiresAt).HasDefaultValueSql("DATEADD(day, 30, GETUTCDATE())");
                
                // Convert enum to string for database storage
                entity.Property(n => n.Type)
                      .HasConversion<string>();
                
                entity.Property(n => n.Priority)
                      .HasConversion<string>();
            });

            // Enum conversions
            modelBuilder.Entity<EmailVerification>()
                .Property(e => e.Status)
                .HasConversion<string>();
        }

       

    }
}
