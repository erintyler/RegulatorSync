using Microsoft.EntityFrameworkCore;
using File = Regulator.Client.Data.Models.File;

namespace Regulator.Client.Data.Contexts;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<File> Files { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<File>()
            .HasKey(f => f.Hash);
    }
}