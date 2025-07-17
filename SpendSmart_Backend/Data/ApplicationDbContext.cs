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

            // Configure Goal entity
            modelBuilder.Entity<Goal>(entity =>
            {
                // Configure decimal precision for monetary values
                entity.Property(e => e.TargetAmount).HasPrecision(18, 2);
                entity.Property(e => e.CurrentAmount).HasPrecision(18, 2);

                // Configure Goal-User relationship with navigation property
                entity.HasOne(g => g.User)
                    .WithMany(u => u.Goals)  // Added navigation property
                    .HasForeignKey(g => g.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure SavingRecord entity
            modelBuilder.Entity<SavingRecord>(entity =>
            {
                // Configure decimal precision for monetary values
                entity.Property(e => e.Amount).HasPrecision(18, 2);

                // Configure SavingRecord-Goal relationship
                entity.HasOne(sr => sr.Goal)
                    .WithMany(g => g.SavingRecords)
                    .HasForeignKey(sr => sr.GoalId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Configure SavingRecord-User relationship with navigation property
                entity.HasOne(sr => sr.User)
                    .WithMany(u => u.SavingRecords)  // Added navigation property
                    .HasForeignKey(sr => sr.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Budget entity
            modelBuilder.Entity<Budget>(entity =>
            {
                entity.Property(b => b.AllocatedAmount).HasPrecision(18, 2);
                entity.Property(b => b.SpendAmount).HasPrecision(18, 2);

                // Configure Budget-User relationship
                entity.HasOne(b => b.User)
                    .WithMany(u => u.Budgets)
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Transaction entity
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.Property(t => t.Amount).HasPrecision(18, 2);

                // Configure Transaction-User relationship
                entity.HasOne(t => t.User)
                    .WithMany(u => u.Transactions)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Configure Transaction-Category relationship (if CategoryId exists)
                entity.HasOne(t => t.Category)
                    .WithMany(c => c.Transactions)
                    .HasForeignKey(t => t.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Configure Transaction-BudgetCategory relationship (if BudgetCategoryId exists)
                entity.HasOne(t => t.BudgetCategory)
                    .WithMany(bc => bc.Transactions)
                    .HasForeignKey(t => t.BudgetCategoryId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Configure BudgetCategory entity relationships to prevent cascade conflicts
            modelBuilder.Entity<BudgetCategory>(entity =>
            {
                // Configure decimal precision for monetary values
                entity.Property(bc => bc.AllocatedAmount).HasPrecision(18, 2);
                entity.Property(bc => bc.RemainingAmount).HasPrecision(18, 2);

                // Configure BudgetCategory-Budget relationship (NO ACTION to prevent cascade conflicts)
                entity.HasOne(bc => bc.Budget)
                    .WithMany(b => b.BudgetCategories)  // Added navigation property
                    .HasForeignKey(bc => bc.BudgetId)
                    .OnDelete(DeleteBehavior.NoAction);

                // Configure BudgetCategory-Category relationship (NO ACTION to prevent cascade conflicts)
                entity.HasOne(bc => bc.Category)
                    .WithMany(c => c.BudgetCategories)  // Added navigation property
                    .HasForeignKey(bc => bc.CategoryId)
                    .OnDelete(DeleteBehavior.NoAction);

                // Configure BudgetCategory-User relationship (NO ACTION to prevent cascade conflicts)
                entity.HasOne(bc => bc.User)
                    .WithMany(u => u.BudgetCategories)  // Added navigation property
                    .HasForeignKey(bc => bc.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Configure Category entity
            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
            });

            // Configure Report entity
            modelBuilder.Entity<Report>(entity =>
            {
                entity.HasOne(r => r.User)
                    .WithMany(u => u.Reports)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

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
    }
}