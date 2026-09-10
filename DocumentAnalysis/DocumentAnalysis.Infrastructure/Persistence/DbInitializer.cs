using Microsoft.EntityFrameworkCore;

namespace DocumentAnalysis.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(DocumentAnalysisDbContext context)
    {
        await context.Database.EnsureCreatedAsync();
        await context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH('AnalyzedDocuments', 'UserId') IS NULL
            BEGIN
                ALTER TABLE AnalyzedDocuments ADD UserId uniqueidentifier NULL;
            END
            """);
    }
}
