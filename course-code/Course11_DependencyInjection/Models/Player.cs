namespace Course11_DependencyInjection.Models;

public sealed class Player
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = "未命名玩家";
}
