// Course 02: 类和对象，把散落变量升级为 Player 对象。

using Course02_PlayerClass.Models;

Player player = new Player
{
    Name = "Alice",
    Level = 10,
    Region = "CN",
    Gold = 500
};

Console.WriteLine(player.GetSummary());
