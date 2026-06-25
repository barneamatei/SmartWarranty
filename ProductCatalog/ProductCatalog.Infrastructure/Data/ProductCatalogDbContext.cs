using Microsoft.EntityFrameworkCore;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Infrastructure.Data;

public class ProductCatalogDbContext : DbContext
{
    public ProductCatalogDbContext(DbContextOptions<ProductCatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }

    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");

            entity.HasKey(e => e.ProductId);

            entity.Property(e => e.ProductId)
                .HasColumnName("ProductId")
                .IsRequired();

            entity.Property(e => e.Name)
                .HasColumnName("Name")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Brand)
                .HasColumnName("Brand")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Model)
                .HasColumnName("Model")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.CategoryId)
                .HasColumnName("CategoryId")
                .IsRequired();

            entity.Property(e => e.UserId)
                .HasColumnName("UserId");

            entity.Property(e => e.Status)
                .HasColumnName("Status")
                .HasConversion<int>()
                .IsRequired();

            entity.HasOne<Category>()
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");

            entity.HasKey(e => e.CategoryId);

            entity.Property(e => e.CategoryId)
                .HasColumnName("CategoryId")
                .IsRequired();

            entity.Property(e => e.Name)
                .HasColumnName("Name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasColumnName("Description")
                .HasMaxLength(500);

            entity.Property(e => e.UserId)
                .HasColumnName("UserId");

            entity.HasIndex(e => e.UserId);
        });
    }
}
