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

            // UserAdmin entity configuration
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

            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Required fields
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);

                // Profile picture fields (NEW)
                entity.Property(e => e.ProfilePictureUrl).HasMaxLength(500);
                entity.Property(e => e.ProfilePicturePath).HasMaxLength(500);

                // Timestamp fields with database defaults
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Indexes
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Your existing entity configurations...
            // (Keep all your existing modelBuilder configurations for other entities)
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<UserAdmin> UserAdmins { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Goal> Goals { get; set; }
        public DbSet<Report> Reports { get; set; }
    }
}
