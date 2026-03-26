using Microsoft.EntityFrameworkCore;
using WarrantyManagement.Domain.Entities;

namespace WarrantyManagement.Infrastructure.Persistence;

public class WarrantyManagementDbContext : DbContext
{
    public WarrantyManagementDbContext(DbContextOptions<WarrantyManagementDbContext> options)
        : base(options)
    {
    }

    public DbSet<Warranty> Warranties { get; set; }

    public DbSet<Claim> Claims { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Warranty>(entity =>
        {
            entity.ToTable("Warranties");
            entity.HasKey(e => e.WarrantyId);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.ProductId).IsRequired();
            entity.Property(e => e.PurchaseDate).IsRequired();
            entity.Property(e => e.DurationMonths).IsRequired();
            entity.Property(e => e.ExpiryDate).IsRequired();
            entity.Property(e => e.Status).HasConversion<int>().IsRequired();

            entity.HasMany(e => e.Claims)
                .WithOne(c => c.Warranty)
                .HasForeignKey(c => c.WarrantyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Claim>(entity =>
        {
            entity.ToTable("Claims");
            entity.HasKey(e => e.ClaimId);
            entity.Property(e => e.WarrantyId).IsRequired();
            entity.Property(e => e.Status).HasConversion<int>().IsRequired();
            entity.Property(e => e.OpenedAt).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000).IsRequired();
        });
    }
}
