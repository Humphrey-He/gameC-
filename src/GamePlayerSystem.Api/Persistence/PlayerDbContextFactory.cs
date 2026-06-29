using GamePlayerSystem.Core.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GamePlayerSystem.Api.Persistence;

public sealed class PlayerDbContextFactory : IDesignTimeDbContextFactory<PlayerDbContext>
{
    public PlayerDbContext CreateDbContext(string[] args)
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        string appSettingsDirectory = ResolveAppSettingsDirectory(currentDirectory);

        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(appSettingsDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        string connectionString = configuration.GetConnectionString("PlayerDatabase")
            ?? throw new InvalidOperationException("缺少 ConnectionStrings:PlayerDatabase 配置");

        string normalizedConnectionString = SqliteDatabasePath.NormalizeConnectionString(
            connectionString,
            currentDirectory);

        DbContextOptions<PlayerDbContext> options = new DbContextOptionsBuilder<PlayerDbContext>()
            .UseSqlite(
                normalizedConnectionString,
                sqliteOptions => sqliteOptions.MigrationsAssembly(typeof(PlayerDbContextFactory).Assembly.GetName().Name))
            .Options;

        return new PlayerDbContext(options);
    }

    private static string ResolveAppSettingsDirectory(string currentDirectory)
    {
        string directAppSettings = Path.Combine(currentDirectory, "appsettings.json");
        if (File.Exists(directAppSettings))
        {
            return currentDirectory;
        }

        string projectAppSettings = Path.Combine(
            currentDirectory,
            "src",
            "GamePlayerSystem.Api",
            "appsettings.json");

        if (File.Exists(projectAppSettings))
        {
            return Path.GetDirectoryName(projectAppSettings)!;
        }

        throw new FileNotFoundException("无法找到 GamePlayerSystem.Api 的 appsettings.json");
    }
}
