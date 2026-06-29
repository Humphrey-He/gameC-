namespace Course16_RepositoryEfCore.Dtos;

public sealed class PagedResult<T>
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public IReadOnlyList<T> Items { get; init; } = [];
}
