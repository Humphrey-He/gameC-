namespace Course13_ApiProjectStandards.Models;

public sealed class Player
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = "未命名玩家";
    public int Level { get; set; } = 1;
    public string Region { get; set; } = "CN";
    public int Gold { get; set; }
}
