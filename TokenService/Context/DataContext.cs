using Microsoft.EntityFrameworkCore;
using TokenService.Models;

namespace TokenService.Context;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions options) : base(options)
    {
    }
    
    public DbSet<RefreshTokenModel> RefreshTokens { get; set; }
}