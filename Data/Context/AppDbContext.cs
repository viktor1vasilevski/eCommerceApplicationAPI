using EntityModels.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> dbContext) : base(dbContext)
    {
            
    }

    public DbSet<User> Users { get; set; }
}
