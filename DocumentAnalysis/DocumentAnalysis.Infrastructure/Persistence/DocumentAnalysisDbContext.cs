using DocumentAnalysis.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocumentAnalysis.Infrastructure.Persistence;

public class DocumentAnalysisDbContext : DbContext
{
    public DocumentAnalysisDbContext(DbContextOptions<DocumentAnalysisDbContext> options)
        : base(options)
    {
    }

    public DbSet<AnalyzedDocument> Documents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AnalyzedDocument>(entity =>
        {
            entity.ToTable("AnalyzedDocuments");
            entity.HasKey(e => e.DocumentId);
            entity.Property(e => e.UserId);
            entity.Property(e => e.OriginalFileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.ContentType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UploadedAt).IsRequired();
            entity.Property(e => e.ExtractedText).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.DocumentType).HasConversion<int>().IsRequired();
            entity.Property(e => e.MerchantName).HasMaxLength(255);
            entity.Property(e => e.DocumentNumber).HasMaxLength(100);
            entity.Property(e => e.Currency).HasMaxLength(16);
            entity.Property(e => e.Status).HasConversion<int>().IsRequired();
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
        });
    }
}
