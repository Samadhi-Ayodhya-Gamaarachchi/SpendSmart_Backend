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

            // Configure UserAdmin relationships
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

            // Configure User indexes
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
            modelBuilder.Entity<SavingRecord>(entity =>
            {
                entity.Property(e => e.Amount).HasPrecision(18, 2);

                entity.HasOne(sr => sr.Goal)
                    .WithMany(g => g.SavingRecords)
                    .HasForeignKey(sr => sr.GoalId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sr => sr.User)
                    .WithMany(u => u.SavingRecords)
                    .HasForeignKey(sr => sr.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Budget entity
            modelBuilder.Entity<Budget>(entity =>
            {
                entity.Property(b => b.TotalBudgetAmount).HasPrecision(18, 2);
                entity.Property(b => b.TotalSpentAmount).HasPrecision(18, 2);

                entity.HasOne(b => b.User)
                    .WithMany(u => u.Budgets)
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasCheckConstraint("CK_Budget_BudgetType", "[BudgetType] IN ('Monthly', 'Annually')");
                entity.HasCheckConstraint("CK_Budget_Status", "[Status] IN ('Active', 'Completed', 'Cancelled')");
            });

            // Configure BudgetCategory entity
            modelBuilder.Entity<BudgetCategory>(entity =>
            {
                entity.Property(bc => bc.AllocatedAmount).HasPrecision(18, 2);
                entity.Property(bc => bc.SpentAmount).HasPrecision(18, 2);

                entity.HasOne(bc => bc.Budget)
                    .WithMany(b => b.BudgetCategories)
                    .HasForeignKey(bc => bc.BudgetId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(bc => bc.Category)
                    .WithMany(c => c.BudgetCategories)
                    .HasForeignKey(bc => bc.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(bc => new { bc.BudgetId, bc.CategoryId })
                    .IsUnique();
            });

            // Configure Transaction entity
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.Property(t => t.Amount).HasPrecision(18, 2);

                entity.HasOne(t => t.User)
                    .WithMany(u => u.Transactions)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(t => t.Category)
                    .WithMany(c => c.Transactions)
                    .HasForeignKey(t => t.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasCheckConstraint("CK_Transaction_TransactionType", "[TransactionType] IN ('Income', 'Expense')");
                entity.HasCheckConstraint("CK_Transaction_RecurringFrequency",
                    "[RecurringFrequency] IN ('Daily', 'Weekly', 'Monthly', 'Annually') OR [RecurringFrequency] IS NULL");
            });

            // Configure TransactionBudgetImpact entity
            modelBuilder.Entity<TransactionBudgetImpact>(entity =>
            {
                entity.Property(tbi => tbi.ImpactAmount).HasPrecision(18, 2);

                entity.HasOne(tbi => tbi.Transaction)
                    .WithMany(t => t.TransactionBudgetImpacts)
                    .HasForeignKey(tbi => tbi.TransactionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(tbi => tbi.Budget)
                    .WithMany(b => b.TransactionBudgetImpacts)
                    .HasForeignKey(tbi => tbi.BudgetId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(tbi => tbi.Category)
                    .WithMany(c => c.TransactionBudgetImpacts)
                    .HasForeignKey(tbi => tbi.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Category entity
            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(c => c.CategoryName).HasMaxLength(100).IsRequired();
                // Only Income or Expense, removed 'Both'
                entity.HasCheckConstraint("CK_Category_Type", "[Type] IN ('Income', 'Expense')");
            });

            // Configure RecurringTransaction entity
            modelBuilder.Entity<RecurringTransaction>(entity =>
            {
                entity.Property(rt => rt.Amount).HasPrecision(18, 2);

                entity.HasOne(rt => rt.User)
                    .WithMany()
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rt => rt.Category)
                    .WithMany()
                    .HasForeignKey(rt => rt.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Report entity
            modelBuilder.Entity<Report>(entity =>
            {
                entity.HasOne(r => r.User)
                    .WithMany(u => u.Reports)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Create indexes for performance
            modelBuilder.Entity<Budget>()
                .HasIndex(b => new { b.UserId, b.Status })
                .HasDatabaseName("IX_Budget_UserId_Status");

            modelBuilder.Entity<Budget>()
                .HasIndex(b => new { b.StartDate, b.EndDate })
                .HasDatabaseName("IX_Budget_DateRange");

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => new { t.UserId, t.TransactionDate })
                .HasDatabaseName("IX_Transaction_UserId_Date");

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => new { t.CategoryId, t.TransactionType })
                .HasDatabaseName("IX_Transaction_Category_Type");

            modelBuilder.Entity<BudgetCategory>()
                .HasIndex(bc => bc.BudgetId)
                .HasDatabaseName("IX_BudgetCategory_BudgetId");

            modelBuilder.Entity<TransactionBudgetImpact>()
                .HasIndex(tbi => tbi.TransactionId)
                .HasDatabaseName("IX_TransactionBudgetImpact_TransactionId");

            modelBuilder.Entity<TransactionBudgetImpact>()
                .HasIndex(tbi => tbi.BudgetId)
                .HasDatabaseName("IX_TransactionBudgetImpact_BudgetId");
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<UserAdmin> UserAdmins { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Goal> Goals { get; set; }
        public DbSet<SavingRecord> SavingRecords { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<BudgetCategory> BudgetCategories { get; set; }
        public DbSet<TransactionBudgetImpact> TransactionBudgetImpacts { get; set; }
        public DbSet<RecurringTransaction> RecurringTransactions { get; set; }
    }
}
