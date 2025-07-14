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

                // Configure Goal-User relationship
                entity.HasOne(g => g.User)
                    .WithMany()
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

                // Configure SavingRecord-User relationship
                entity.HasOne(sr => sr.User)
                    .WithMany()
                    .HasForeignKey(sr => sr.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Budget entity (if it exists)
            modelBuilder.Entity<Budget>(entity =>
            {
                entity.Property(b => b.AllocatedAmount).HasPrecision(18, 2);
                entity.Property(b => b.SpendAmount).HasPrecision(18, 2);
            });

            // Configure Transaction entity
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.Property(t => t.Amount).HasPrecision(18, 2);
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
    }
}