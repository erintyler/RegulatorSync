using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Regulator.Client.Data.Contexts;

namespace Regulator.Client.Data.Services;

public class DesignTimeAppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Regulator", "regulator_client.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
        
        return new AppDbContext(optionsBuilder.Options);
    }
}