using Course16_RepositoryEfCore.Models;
using Course16_RepositoryEfCore.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Course16_RepositoryEfCore.Repositories;

public sealed class EfPlayerRepository : IPlayerRepository
{
    private readonly PlayerDbContext _dbContext;

    public EfPlayerRepository(PlayerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Player player, CancellationToken cancellationToken = default)
    {
        await _dbContext.Players.AddAsync(player, cancellationToken);
    }

    public Task<Player?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Players
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public Task<Player?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Players
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public Task<List<Player>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        int skip = (pageNumber - 1) * pageSize;

        return _dbContext.Players
            .AsNoTracking()
            .OrderByDescending(p => p.Level)
            .ThenBy(p => p.Id)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Player>> GetByRegionAsync(
        string region,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Players
            .AsNoTracking()
            .Where(p => p.Region == region)
            .OrderByDescending(p => p.Level)
            .ThenBy(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Player>> SearchByNameAsync(
        string keyword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return Task.FromResult(new List<Player>());
        }

        return _dbContext.Players
            .AsNoTracking()
            .Where(p => p.Name.Contains(keyword))
            .OrderByDescending(p => p.Level)
            .ThenBy(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Players
            .AsNoTracking()
            .AnyAsync(p => p.Name == name, cancellationToken);
    }

    public void Remove(Player player)
    {
        _dbContext.Players.Remove(player);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
