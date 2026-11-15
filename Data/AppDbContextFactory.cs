using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ApiClinica.Data;

// Permite executar comandos EF sem exigir segredos JWT em tempo de design.
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(configuration.GetConnectionString("DefaultConnection"))
            .Options;
        return new AppDbContext(options);
    }
}
