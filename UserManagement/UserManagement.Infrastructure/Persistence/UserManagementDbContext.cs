using Microsoft.EntityFrameworkCore;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Persistence;

public class UserManagementDbContext : DbContext
{
    public UserManagementDbContext(DbContextOptions<UserManagementDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<UserProfile> UserProfiles { get; set; }

    public DbSet<Subscription> Subscriptions { get; set; }

    public DbSet<FamilyShare> FamilyShares { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.Status).HasConversion<int>().IsRequired();

            entity.HasOne(e => e.UserProfile)
                .WithOne()
                .HasForeignKey<UserProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Subscription)
                .WithOne()
                .HasForeignKey<Subscription>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.ToTable("UserProfiles");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Language).HasMaxLength(10);
            entity.Property(e => e.Preferences).HasMaxLength(500);
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.ToTable("Subscriptions");
            entity.HasKey(e => e.SubscriptionId);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.PlanType).HasConversion<int>().IsRequired();
            entity.Property(e => e.StartDate).IsRequired();
            entity.Property(e => e.EndDate).IsRequired();
        });

        modelBuilder.Entity<FamilyShare>(entity =>
        {
            entity.ToTable("FamilyShares");
            entity.HasKey(e => e.ShareId);
            entity.Property(e => e.OwnerUserId).IsRequired();
            entity.Property(e => e.MemberUserId).IsRequired();
            entity.Property(e => e.Permissions).HasConversion<int>().IsRequired();

            entity.HasIndex(e => new { e.OwnerUserId, e.MemberUserId }).IsUnique();
        });
    }
}
