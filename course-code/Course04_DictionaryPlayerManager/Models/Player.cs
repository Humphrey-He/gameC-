namespace Course04_DictionaryPlayerManager.Models;

public sealed class Player
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = "未命名玩家";
    public int Level { get; set; } = 1;
    public int Gold { get; set; }
    public bool IsActive { get; set; } = true;

    public int CalculatePower()
    {
        return Level * 100 + Gold / 10;
    }
}
