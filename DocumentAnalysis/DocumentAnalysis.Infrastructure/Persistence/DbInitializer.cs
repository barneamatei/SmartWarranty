using DocumentAnalysis.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocumentAnalysis.Infrastructure.Persistence;

public static class DbInitializer
{
    private static readonly Guid AnaUserId = Guid.Parse("3a69d535-56b5-47f1-b052-a4ff3ba264bf");
    private static readonly Guid MihaiUserId = Guid.Parse("4c3734dd-925c-4548-a232-63af2f0d4220");
    private static readonly Guid ElenaUserId = Guid.Parse("0c00a9da-4c90-434e-b375-b52db5079db0");

    public static async Task InitializeAsync(DocumentAnalysisDbContext context)
    {
        await context.Database.EnsureCreatedAsync();
        await context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH('AnalyzedDocuments', 'UserId') IS NULL
            BEGIN
                ALTER TABLE AnalyzedDocuments ADD UserId uniqueidentifier NULL;
            END
            """);

        await EnsureDocumentAsync(
            context,
            Guid.Parse("76000000-0000-0000-0000-000000000001"),
            AnaUserId,
            "emag-iphone-15-pro.pdf",
            "eMAG",
            "EMG-2026-0703",
            new DateTime(2024, 7, 3),
            6299.99m,
            "iPhone 15 Pro, 256GB, Natural Titanium");

        await EnsureDocumentAsync(
            context,
            Guid.Parse("76000000-0000-0000-0000-000000000002"),
            AnaUserId,
            "altex-delonghi-magnifica.pdf",
            "Altex",
            "ATX-2023-0315",
            new DateTime(2023, 3, 15),
            2199.90m,
            "Espressor automat DeLonghi Magnifica Evo");

        await EnsureDocumentAsync(
            context,
            Guid.Parse("76000000-0000-0000-0000-000000000003"),
            MihaiUserId,
            "pcgarage-dell-ultrasharp.pdf",
            "PC Garage",
            "PCG-2024-0718",
            new DateTime(2024, 7, 18),
            2899.00m,
            "Monitor Dell UltraSharp U2723QE");

        await EnsureDocumentAsync(
            context,
            Guid.Parse("76000000-0000-0000-0000-000000000004"),
            ElenaUserId,
            "f64-sony-alpha-7iv.pdf",
            "F64",
            "F64-2025-0812",
            new DateTime(2025, 8, 12),
            11999.00m,
            "Camera Sony Alpha 7 IV body");

        await context.SaveChangesAsync();
    }

    private static async Task EnsureDocumentAsync(
        DocumentAnalysisDbContext context,
        Guid documentId,
        Guid userId,
        string fileName,
        string merchant,
        string documentNumber,
        DateTime issueDate,
        decimal totalAmount,
        string productLine)
    {
        if (await context.Documents.AnyAsync(item => item.DocumentId == documentId))
            return;

        var document = new AnalyzedDocument(documentId, fileName, "application/pdf", userId);
        document.MarkProcessed(
            $"""
            Furnizor: {merchant}
            Document: {documentNumber}
            Data: {issueDate:yyyy-MM-dd}
            Produs: {productLine}
            Total: {totalAmount:0.00} RON
            Garantie comerciala inclusa conform certificatului atasat.
            """,
            DocumentType.Invoice,
            merchant,
            documentNumber,
            issueDate,
            totalAmount,
            "RON",
            usedOcr: false);

        await context.Documents.AddAsync(document);
    }
}
