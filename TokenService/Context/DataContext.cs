using Microsoft.EntityFrameworkCore;
using TokenService.Models;
using TokenService.Models.DataModels;

namespace TokenService.Context;

public class DataContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<RefreshToken> Tokens { get; set; }
}