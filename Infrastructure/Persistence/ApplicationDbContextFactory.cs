using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var currentDir = Directory.GetCurrentDirectory();

        // If current dir already ends with "Api", use it.
        // Otherwise, assume solution root and append "Api".
        var apiDir = Path.GetFileName(currentDir).Equals("Api", StringComparison.OrdinalIgnoreCase)
            ? currentDir
            : Path.Combine(currentDir, "Api");

        // Extra safety: if still not found, try one level up (common when EF runs from bin folders)
        if (!Directory.Exists(apiDir))
        {
            var parent = Directory.GetParent(currentDir)?.FullName;
            if (parent is not null)
            {
                var candidate = Path.Combine(parent, "Api");
                if (Directory.Exists(candidate))
                    apiDir = candidate;
            }
        }

        if (!Directory.Exists(apiDir))
            throw new DirectoryNotFoundException($"Api directory not found. CurrentDir='{currentDir}', Tried='{apiDir}'");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiDir)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
