// Course 15: EF Core + SQLite 最小可运行示例。

using Course15_EfCoreSqlite.Models;
using Course15_EfCoreSqlite.Persistence;
using Course15_EfCoreSqlite.Services;
using Microsoft.EntityFrameworkCore;

string databasePath = Path.Combine("data", "course15-players.db");
Directory.CreateDirectory("data");

DbContextOptions<PlayerDbContext> options = new DbContextOptionsBuilder<PlayerDbContext>()
    .UseSqlite($"Data Source={databasePath}")
    .Options;

await using PlayerDbContext dbContext = new PlayerDbContext(options);
await dbContext.Database.EnsureCreatedAsync();

EfPlayerStorage storage = new EfPlayerStorage(dbContext);

List<Player> players =
[
    new Player { Name = "Alice", Level = 10, Region = "CN", Gold = 500 },
    new Player { Name = "Bob", Level = 25, Region = "NA", Gold = 1200 }
];

await storage.SaveAsync(players);

List<Player> loadedPlayers = await storage.LoadAsync();

Console.WriteLine($"SQLite 数据库路径：{databasePath}");
Console.WriteLine($"已加载玩家数量：{loadedPlayers.Count}");
