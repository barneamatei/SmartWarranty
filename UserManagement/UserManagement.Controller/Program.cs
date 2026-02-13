using Microsoft.EntityFrameworkCore;
using UserManagement.Service.Exceptions;
using UserManagement.Service.Services;
using UserManagement.Domain.Contracts;
using UserManagement.Infrastructure.Persistence;
using UserManagement.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "User Management API",
        Version = "v1",
        Description = "API pentru gestiunea utilizatorilor, abonamentelor și partajării în familie."
    });
});

var connectionString = builder.Configuration.GetConnectionString("UserManagementDb");
builder.Services.AddDbContext<UserManagementDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFamilyShareRepository, FamilyShareRepository>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFamilyShareService, FamilyShareService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();

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
            FamilyShareAlreadyExistsException fsEx => (StatusCodes.Status409Conflict, fsEx.Message),
            _ => (StatusCodes.Status500InternalServerError, "An internal error occurred.")
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new { error = message });
    });
});

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "User Management API v1");
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<UserManagementDbContext>();
    await context.Database.EnsureCreatedAsync();
}

app.Run();
