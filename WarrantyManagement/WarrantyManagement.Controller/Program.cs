using Microsoft.EntityFrameworkCore;
using WarrantyManagement.Domain.Contracts;
using WarrantyManagement.Infrastructure.Data;
using WarrantyManagement.Infrastructure.Clients;
using WarrantyManagement.Infrastructure.Persistence;
using WarrantyManagement.Infrastructure.Repositories;
using WarrantyManagement.Service.Exceptions;
using WarrantyManagement.Service.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Warranty Management API",
        Version = "v1",
        Description = "API pentru gestiunea garantiilor si a cererilor de garantie."
    });
});

var connectionString = builder.Configuration.GetConnectionString("WarrantyManagementDb");
builder.Services.AddDbContext<WarrantyManagementDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IWarrantyDao, WarrantyRepository>();
builder.Services.AddScoped<IClaimDao, ClaimRepository>();
builder.Services.AddHttpClient<IUserManagementClient, UserManagementClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalServices:UserManagementBaseUrl"] ?? "http://localhost:5139/");
});
builder.Services.AddHttpClient<IProductCatalogClient, ProductCatalogClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalServices:ProductCatalogBaseUrl"] ?? "http://localhost:5085/");
});
builder.Services.AddHttpClient<INotificationManagementClient, NotificationManagementClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalServices:NotificationManagementBaseUrl"] ?? "http://localhost:5160/");
});

var documentAnalysisBaseUrl = builder.Configuration["ExternalServices:DocumentAnalysisBaseUrl"] ?? "http://localhost:5291/";
builder.Services.AddHttpClient<IDocumentAnalysisClient, DocumentAnalysisClient>(client =>
{
    client.BaseAddress = new Uri(documentAnalysisBaseUrl);
});

builder.Services.AddScoped<WarrantyService>();
builder.Services.AddScoped<ClaimService>();

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
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Warranty Management API v1");
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<WarrantyManagementDbContext>();
    await DbInitializer.InitializeAsync(context);
}

app.Run();
