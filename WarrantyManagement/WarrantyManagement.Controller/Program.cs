using Microsoft.EntityFrameworkCore;
using WarrantyManagement.Domain.Contracts;
using WarrantyManagement.Infrastructure.Data;
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

builder.Services.AddScoped<IWarrantyService, WarrantyService>();
builder.Services.AddScoped<IClaimService, ClaimService>();

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
