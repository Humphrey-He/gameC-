// Course 03: 使用 List<Player> 管理多个玩家。

using Course03_ListPlayerManager.Models;
using Course03_ListPlayerManager.Services;

PlayerManager manager = new PlayerManager();

manager.AddPlayer(new Player { Name = "Alice", Level = 10, Region = "CN", Gold = 500 });
manager.AddPlayer(new Player { Name = "Bob", Level = 25, Region = "NA", Gold = 1200 });
manager.AddPlayer(new Player { Name = "Cindy", Level = 35, Region = "EU", Gold = 3000 });

Console.WriteLine("全部玩家：");
foreach (Player player in manager.Players)
{
    Console.WriteLine(player.GetSummary());
}

Console.WriteLine();
Console.WriteLine("战力 Top 2：");
foreach (Player player in manager.GetTopByPower(2))
{
    Console.WriteLine(player.GetSummary());
}
