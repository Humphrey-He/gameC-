// Course 06: 文件读写与 JSON 持久化。

using Course06_JsonStorage.Models;
using Course06_JsonStorage.Services;

string filePath = Path.Combine("data", "course06-players.json");
PlayerStorage storage = new PlayerStorage(filePath);

List<Player> players =
[
    new Player { Name = "Alice", Level = 10, Region = "CN", Gold = 500 },
    new Player { Name = "Bob", Level = 25, Region = "NA", Gold = 1200 }
];

storage.Save(players);

List<Player> loadedPlayers = storage.Load();

Console.WriteLine($"保存并加载成功：{loadedPlayers.Count} 个玩家");
foreach (Player player in loadedPlayers)
{
    Console.WriteLine($"{player.Name} Lv.{player.Level} {player.Region}");
}
