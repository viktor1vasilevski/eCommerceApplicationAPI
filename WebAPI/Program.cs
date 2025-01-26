using Data.Context;
using EntityModels.Enums;
using EntityModels.Models;
using Main.Helpers;
using Microsoft.EntityFrameworkCore;
using Main.Extensions;
using FluentValidation;
using Main.Validations.Auth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(policy => policy.AddPolicy("MyPolicy", builder =>
{
    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
}));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIoCService();
builder.Services.AddValidatorsFromAssemblyContaining<UserRegisterRequestValidator>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;

    try
    {
        var dbContext = serviceProvider.GetRequiredService<AppDbContext>();

        // Apply any pending migrations
        dbContext.Database.Migrate();

        var adminExist = dbContext.Users.Any(x => x.Role == Role.Admin);

        if (!adminExist)
        {

            var saltKey = PasswordHasher.GenerateSalt();
            var adminUser = new User
            {
                FirstName = "Admin",
                LastName = "Admin",
                Email = "admin@example.com",
                Username = "admin",
                Role = Role.Admin,
                PasswordHash = PasswordHasher.HashPassword("Admin@123", saltKey),
                SaltKey = Convert.ToBase64String(saltKey),
                CreatedBy = "Admin",
                Created = DateTime.Now,
                LastModifiedBy = null,
                LastModified = null
            };

            dbContext.Users.Add(adminUser);
            dbContext.SaveChanges();

        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during admin user setup: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("MyPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
