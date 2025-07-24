using Microsoft.EntityFrameworkCore;
using AuthApi.Models;

namespace AuthApi.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<BmiData> BmiData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User tablosu yapılandırması
            modelBuilder.Entity<User>(entity =>
            {
                // Primary Key
                entity.HasKey(e => e.Id);

                // Username unique olmalı
                entity.HasIndex(e => e.Username)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_Username");

                // Email unique olmalı
                entity.HasIndex(e => e.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_Email");

                // Özellik yapılandırmaları
                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.PasswordHash)
                    .IsRequired();

                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasDefaultValue("User");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("datetime('now')");

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);
            });

            // BmiData tablosu yapılandırması
            modelBuilder.Entity<BmiData>(entity =>
            {
                // Primary Key
                entity.HasKey(e => e.Id);

                // Foreign Key - User ile ilişki
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // UserId için index (performans için)
                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("IX_BmiData_UserId");

                // Her kullanıcı için sadece bir BMI kaydı (unique constraint)
                entity.HasIndex(e => e.UserId)
                    .IsUnique()
                    .HasDatabaseName("IX_BmiData_UserId_Unique");

                // Özellik yapılandırmaları
                entity.Property(e => e.Height)
                    .IsRequired()
                    .HasPrecision(5, 2); // 5 digit, 2 decimal (örn: 999.99)

                entity.Property(e => e.Weight)
                    .IsRequired()
                    .HasPrecision(6, 2); // 6 digit, 2 decimal (örn: 9999.99)

                entity.Property(e => e.Gender)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.BmiValue)
                    .IsRequired()
                    .HasPrecision(5, 2); // 5 digit, 2 decimal (örn: 999.99)

                entity.Property(e => e.BmiCategory)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("datetime('now')");
            });
        }
    }
}