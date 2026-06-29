namespace gameC_.Models;

public sealed class Player
{
    private int _level = 1;
    private int _gold;

    public Guid Id { get; init; } = Guid.NewGuid();

    public string Name { get; set; } = "未命名玩家";

    public int Level
    {
        get => _level;
        set => _level = value < 1 ? 1 : value;
    }

    public string Region { get; set; } = "CN";

    public int Gold
    {
        get => _gold;
        set => _gold = value < 0 ? 0 : value;
    }

    public bool IsActive { get; set; } = true;
}
