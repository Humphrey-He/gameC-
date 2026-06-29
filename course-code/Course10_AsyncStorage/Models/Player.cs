namespace Course10_AsyncStorage.Models;

public sealed class Player
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = "未命名玩家";
    public int Level { get; set; } = 1;
}
