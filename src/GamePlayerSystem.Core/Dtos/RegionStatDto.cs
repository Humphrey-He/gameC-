namespace GamePlayerSystem.Core.Dtos;

public sealed class RegionStatDto
{
    public string Region { get; init; } = string.Empty;

    public string RegionName { get; init; } = string.Empty;

    public int PlayerCount { get; init; }

    public int ActivePlayerCount { get; init; }
}
