// Course 16: Repository 模式与 EF Core 查询最小可运行示例。

using Course16_RepositoryEfCore.Models;
using Course16_RepositoryEfCore.Persistence;
using Course16_RepositoryEfCore.Repositories;
using Course16_RepositoryEfCore.Services;
using Microsoft.EntityFrameworkCore;

string databasePath = Path.Combine("data", "course16-players.db");
Directory.CreateDirectory("data");

DbContextOptions<PlayerDbContext> options = new DbContextOptionsBuilder<PlayerDbContext>()
    .UseSqlite($"Data Source={databasePath}")
    .Options;

await using PlayerDbContext dbContext = new PlayerDbContext(options);
await dbContext.Database.EnsureDeletedAsync();
await dbContext.Database.EnsureCreatedAsync();

IPlayerRepository repository = new EfPlayerRepository(dbContext);
PlayerApplication application = new PlayerApplication(repository);

await application.AddPlayerAsync(new Player { Name = "Alice", Level = 10, Region = "CN", Gold = 500 });
await application.AddPlayerAsync(new Player { Name = "Bob", Level = 25, Region = "NA", Gold = 1200 });
await application.AddPlayerAsync(new Player { Name = "Cindy", Level = 35, Region = "CN", Gold = 3000 });

var page = await application.GetPlayersAsync(pageNumber: 1, pageSize: 2);

Console.WriteLine($"SQLite 数据库路径：{databasePath}");
Console.WriteLine($"分页结果：第 {page.PageNumber} 页，每页 {page.PageSize} 条");

foreach (var player in page.Items)
{
    Console.WriteLine($"{player.Name} Lv.{player.Level} {player.Region}");
}
