using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regulator.Client.Data.Contexts;

namespace Regulator.Client.Data.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(o =>
        {
            // Use SQLite database
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Regulator", "regulator_client.db");
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
            
            o.UseSqlite($"Data Source={dbPath}");
        });
        
        services.AddHostedService<Services.DatabaseMigrationService>();
        
        return services;
    }
}