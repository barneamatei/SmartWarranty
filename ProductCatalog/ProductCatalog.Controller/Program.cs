using Microsoft.EntityFrameworkCore;
using ProductCatalog.Domain.Contracts;
using ProductCatalog.Service;
using ProductCatalog.Infrastructure.Data;
using ProductCatalog.Infrastructure.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("ProductCatalogDb");
builder.Services.AddDbContext<ProductCatalogDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IProductDao, ProductRepository>();
builder.Services.AddScoped<ICategoryDao, CategoryRepository>();

builder.Services.AddScoped<ProductCatalogService>();
builder.Services.AddScoped<CategoryCatalogService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.ContentType = "application/json";
        var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;

        var (statusCode, message) = exception switch
        {
            ArgumentException argEx => (StatusCodes.Status400BadRequest, argEx.Message),
            InvalidOperationException invEx => (StatusCodes.Status400BadRequest, invEx.Message),
            _ => (StatusCodes.Status500InternalServerError, "An internal error occurred.")
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new { error = message });
    });
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ProductCatalogDbContext>();
        await ProductCatalog.Infrastructure.Data.DbInitializer.InitializeAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();



