using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence;

public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var apiDir = FindApiDirectory();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiDir)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            // Optional: helps if you move secrets out of appsettings later
            // .AddUserSecrets<ApplicationDbContextFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }

    private static string FindApiDirectory()
    {
        var candidates = new List<string>();

        // 1) Current directory
        var currentDir = Directory.GetCurrentDirectory();
        candidates.Add(currentDir);
        candidates.Add(Path.Combine(currentDir, "Api"));

        // 2) App base directory (often bin/Debug/... during tooling)
        var baseDir = AppContext.BaseDirectory;
        candidates.Add(baseDir);
        candidates.Add(Path.Combine(baseDir, "Api"));

        // 3) One level up variants
        var parent = Directory.GetParent(currentDir)?.FullName;
        if (parent is not null)
        {
            candidates.Add(parent);
            candidates.Add(Path.Combine(parent, "Api"));
        }

        // Pick first existing directory that looks like the Api project root
        foreach (var path in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!Directory.Exists(path)) continue;

            // Heuristic: Api folder should contain appsettings.json
            var appsettings = Path.Combine(path, "appsettings.json");
            if (File.Exists(appsettings))
                return path;
        }

        throw new DirectoryNotFoundException(
            $"Api directory not found. Tried: {string.Join(" | ", candidates)}");
    }
}
