using Microsoft.EntityFrameworkCore;
using NotificationManagement.Domain.Entities;

namespace NotificationManagement.Infrastructure.Persistence;

public class NotificationManagementDbContext : DbContext
{
    public NotificationManagementDbContext(DbContextOptions<NotificationManagementDbContext> options) : base(options)
    {
    }

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(x => x.NotificationId);
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.Metadata).HasMaxLength(2000);
            entity.Property(x => x.ErrorMessage).HasMaxLength(1000);
        });
    }
}
