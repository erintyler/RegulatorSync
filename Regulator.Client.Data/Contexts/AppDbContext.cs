using Microsoft.EntityFrameworkCore;
using Regulator.Client.Data.Models;

namespace Regulator.Client.Data.Contexts;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<DownloadedFile> Files { get; set; }
    public DbSet<UploadedFile> UploadedFiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<DownloadedFile>()
            .HasKey(f => f.Hash);
        
        modelBuilder.Entity<UploadedFile>()
            .HasKey(f => f.Hash);
    }
}