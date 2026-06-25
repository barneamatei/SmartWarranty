using DocumentAnalysis.Domain.Contracts;
using DocumentAnalysis.Domain.Exceptions;
using DocumentAnalysis.Infrastructure.Configuration;
using DocumentAnalysis.Infrastructure.Persistence;
using DocumentAnalysis.Infrastructure.Repositories;
using DocumentAnalysis.Infrastructure.Tasks;
using DocumentAnalysis.Service.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Document Analysis API",
        Version = "v1",
        Description = "API pentru analiza de bonuri si facturi din PDF si imagini."
    });
});

var connectionString = builder.Configuration.GetConnectionString("DocumentAnalysisDb");
builder.Services.AddDbContext<DocumentAnalysisDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.Configure<TesseractOptions>(builder.Configuration.GetSection("Tesseract"));

builder.Services.AddScoped<IAnalyzedDocumentDao, AnalyzedDocumentRepository>();
builder.Services.AddScoped<DocumentAnalysisService>();
builder.Services.AddScoped<IDocumentMetadataExtractor, DocumentMetadataExtractor>();
builder.Services.AddScoped<IImagePreprocessor, ImagePreprocessor>();
builder.Services.AddScoped<IDocumentTextExtractor, PdfDocumentTextExtractor>();
builder.Services.AddScoped<IDocumentTextExtractor, TesseractImageTextExtractor>();

var app = builder.Build();

app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.ContentType = "application/json";
        var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;

        var (statusCode, message) = exception switch
        {
            DomainException domainEx => (StatusCodes.Status400BadRequest, domainEx.Message),
            _ => (StatusCodes.Status500InternalServerError, "An internal error occurred.")
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new { error = message });
    });
});

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Document Analysis API v1");
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DocumentAnalysisDbContext>();
    await DbInitializer.InitializeAsync(context);
}

app.Run();
