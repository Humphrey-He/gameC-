using GamePlayerSystem.Core.Dtos;
using GamePlayerSystem.Core.Models;
using GamePlayerSystem.Core.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GamePlayerSystem.Core.Repositories;

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
            .FirstOrDefaultAsync(player => player.Id == id, cancellationToken);
    }

    public Task<Player?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Players
            .FirstOrDefaultAsync(player => player.Id == id, cancellationToken);
    }

    public Task<List<Player>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        int skip = (pageNumber - 1) * pageSize;

        return OrderedPlayers()
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Player>> GetByRegionAsync(
        string region,
        CancellationToken cancellationToken = default)
    {
        return OrderedPlayers()
            .Where(player => player.IsActive && player.Region == region)
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

        string likePattern = $"%{keyword.Trim()}%";

        return OrderedPlayers()
            .Where(player => EF.Functions.Like(player.Name, likePattern))
            .ToListAsync(cancellationToken);
    }

    public Task<List<Player>> GetTopByPowerAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        count = Math.Clamp(count, 1, 100);

        return _dbContext.Players
            .AsNoTracking()
            .Where(player => player.IsActive)
            .OrderByDescending(player => player.Level * 100 + player.Gold / 10)
            .ThenBy(player => player.Id)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<RegionStatDto>> GetRegionStatsAsync(CancellationToken cancellationToken = default)
    {
        var stats = await _dbContext.Players
            .AsNoTracking()
            .GroupBy(player => player.Region)
            .Select(group => new
            {
                Region = group.Key,
                PlayerCount = group.Count(),
                ActivePlayerCount = group.Count(player => player.IsActive)
            })
            .OrderByDescending(stat => stat.PlayerCount)
            .ThenBy(stat => stat.Region)
            .ToListAsync(cancellationToken);

        return stats
            .Select(stat => new RegionStatDto
            {
                Region = stat.Region,
                RegionName = Player.GetRegionName(stat.Region),
                PlayerCount = stat.PlayerCount,
                ActivePlayerCount = stat.ActivePlayerCount
            })
            .ToList();
    }

    public Task<bool> ExistsByNameAsync(
        string name,
        Guid? ignoredPlayerId = null,
        CancellationToken cancellationToken = default)
    {
        string trimmedName = name.Trim();

        return _dbContext.Players
            .AsNoTracking()
            .AnyAsync(player =>
                player.Name == trimmedName &&
                (ignoredPlayerId == null || player.Id != ignoredPlayerId.Value),
                cancellationToken);
    }

    public void Remove(Player player)
    {
        _dbContext.Players.Remove(player);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IOrderedQueryable<Player> OrderedPlayers()
    {
        return _dbContext.Players
            .AsNoTracking()
            .OrderByDescending(player => player.Level)
            .ThenBy(player => player.Id);
    }
}
