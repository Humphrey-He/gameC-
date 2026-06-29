namespace GamePlayerSystem.Core.Dtos;

public sealed class CreatePlayerRequest
{
    public string Name { get; init; } = string.Empty;

    public int Level { get; init; }

    public string Region { get; init; } = "CN";

    public int Gold { get; init; }
}
