using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Models;

namespace SpendSmart_Backend.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<UserAdmin>()
                .HasKey(ua => new { ua.UserId, ua.ManagerId });
            // Add any additional configuration here
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

            modelBuilder.Entity<Goal>(entity =>
            {
                entity.Property(e => e.TargetAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CurrentAmount).HasColumnType("decimal(18,2)");
            });

            // Configure SavingRecord entity
            modelBuilder.Entity<SavingRecord>(entity =>
            {
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");

                // Configure relationships
                entity.HasOne(sr => sr.Goal)
                    .WithMany()
                    .HasForeignKey(sr => sr.GoalId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sr => sr.User)
                    .WithMany()
                    .HasForeignKey(sr => sr.UserId)
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

        
    }
    
}
