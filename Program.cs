using gameC_.Models;

Player player = new Player
{
    Name = "Alice",
    Level = 10,
    Region = "CN",
    Gold = 500,
    IsActive = true
};

Console.WriteLine($"{player.Name} Lv.{player.Level} {player.Region}");
