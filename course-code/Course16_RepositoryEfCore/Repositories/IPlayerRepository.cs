using Course16_RepositoryEfCore.Models;

namespace Course16_RepositoryEfCore.Repositories;

public interface IPlayerRepository
{
    Task AddAsync(Player player, CancellationToken cancellationToken = default);

    Task<Player?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Player?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<Player>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<List<Player>> GetByRegionAsync(
        string region,
        CancellationToken cancellationToken = default);

    Task<List<Player>> SearchByNameAsync(
        string keyword,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    void Remove(Player player);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
