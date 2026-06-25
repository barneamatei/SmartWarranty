using Microsoft.EntityFrameworkCore;
using NotificationManagement.Domain.Contracts;
using NotificationManagement.Infrastructure.Data;
using NotificationManagement.Infrastructure.Persistence;
using NotificationManagement.Infrastructure.Repositories;
using NotificationManagement.Service.Exceptions;
using NotificationManagement.Service.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Notification Management API",
        Version = "v1",
        Description = "API pentru gestiunea notificarilor din aplicatia SmartWarranty."
    });
});

var connectionString = builder.Configuration.GetConnectionString("NotificationManagementDb");
builder.Services.AddDbContext<NotificationManagementDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<INotificationDao, NotificationRepository>();
builder.Services.AddScoped<NotificationService>();

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
            InvalidOperationException invalidOperationEx => (StatusCodes.Status400BadRequest, invalidOperationEx.Message),
            _ => (StatusCodes.Status500InternalServerError, "An internal error occurred.")
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new { error = message });
    });
});

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification Management API v1");
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<NotificationManagementDbContext>();
    await DbInitializer.InitializeAsync(context);
}

app.Run();
