namespace Course05_LinqAndDtos.Dtos;

public sealed class RankingPlayerDto
{
    public int Rank { get; init; }
    public Guid PlayerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string RegionName { get; init; } = string.Empty;
    public int Level { get; init; }
    public int Power { get; init; }
}
