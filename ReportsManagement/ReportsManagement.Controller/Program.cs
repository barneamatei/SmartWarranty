using ReportsManagement.Domain.Contracts;
using ReportsManagement.Infrastructure.Clients;
using ReportsManagement.Infrastructure.Export;
using ReportsManagement.Service.Exceptions;
using ReportsManagement.Service.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Reports Management API",
        Version = "v1",
        Description = "API pentru rapoarte agregate PDF si Excel generate fara baza de date proprie."
    });
});

var userManagementBaseUrl = builder.Configuration["ExternalServices:UserManagementBaseUrl"] ?? "http://localhost:5139/";
var warrantyManagementBaseUrl = builder.Configuration["ExternalServices:WarrantyManagementBaseUrl"] ?? "http://localhost:5281/";
var productCatalogBaseUrl = builder.Configuration["ExternalServices:ProductCatalogBaseUrl"] ?? "http://localhost:5085/";

builder.Services.AddHttpClient<IUserManagementClient, UserManagementClient>(client =>
{
    client.BaseAddress = new Uri(userManagementBaseUrl);
});

builder.Services.AddHttpClient<IWarrantyManagementClient, WarrantyManagementClient>(client =>
{
    client.BaseAddress = new Uri(warrantyManagementBaseUrl);
});

builder.Services.AddHttpClient<IProductCatalogClient, ProductCatalogClient>(client =>
{
    client.BaseAddress = new Uri(productCatalogBaseUrl);
});

builder.Services.AddScoped<IReportExporter, PdfReportExporter>();
builder.Services.AddScoped<IReportExporter, ExcelReportExporter>();
builder.Services.AddScoped<ReportService>();

var app = builder.Build();

app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.ContentType = "application/json";
        var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;

        var (statusCode, message) = exception switch
        {
            DomainException domainException => (StatusCodes.Status400BadRequest, domainException.Message),
            HttpRequestException httpRequestException => (StatusCodes.Status502BadGateway, httpRequestException.Message),
            _ => (StatusCodes.Status500InternalServerError, "An internal error occurred.")
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new { error = message });
    });
});

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Reports Management API v1");
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
