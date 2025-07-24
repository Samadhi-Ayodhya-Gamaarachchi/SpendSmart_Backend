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

             modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.UserId)
                .IsRequired();
                
            entity.Property(e => e.Rating)
                .IsRequired()
                .HasDefaultValue(0);
                
            entity.Property(e => e.Comment)
                .HasMaxLength(2000);
                
            entity.Property(e => e.PageContext)
                .HasMaxLength(100);
                
            entity.Property(e => e.SubmittedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()"); // SQL Server syntax, adjust for other DBs
                
            entity.Property(e => e.IsProcessed)
                .IsRequired()
                .HasDefaultValue(false);
                
            // Create index for performance
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_Feedback_UserId");
                
            entity.HasIndex(e => e.SubmittedAt)
                .HasDatabaseName("IX_Feedback_SubmittedAt");
                
            entity.HasIndex(e => e.Rating)
                .HasDatabaseName("IX_Feedback_Rating");
                
            entity.HasIndex(e => e.PageContext)
                .HasDatabaseName("IX_Feedback_PageContext");
            
            // Configure foreign key relationship
            entity.HasOne(f => f.User)
                .WithMany() // Adjust if you have a navigation property in User model
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade); // CUSTOMIZE: Change to Restrict if you don't want cascade delete
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
         public DbSet<Feedback> Feedbacks { get; set; }
    }
}
