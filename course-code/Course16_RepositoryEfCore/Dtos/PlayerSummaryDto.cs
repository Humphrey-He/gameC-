namespace Course16_RepositoryEfCore.Dtos;

public sealed class PlayerSummaryDto
{
    public Guid PlayerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Region { get; init; } = string.Empty;
    public int Level { get; init; }
    public bool IsActive { get; init; }
}
