using Course15_EfCoreSqlite.Models;
using Course15_EfCoreSqlite.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Course15_EfCoreSqlite.Services;

public sealed class EfPlayerStorage
{
    private readonly PlayerDbContext _dbContext;

    public EfPlayerStorage(PlayerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Player>> LoadAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Players
            .AsNoTracking()
            .OrderByDescending(p => p.Level)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveAsync(
        IEnumerable<Player> players,
        CancellationToken cancellationToken = default)
    {
        // 兼容 JSON 时代的整体保存接口；真实项目后续应改为 Repository。
        _dbContext.Players.RemoveRange(_dbContext.Players);
        await _dbContext.Players.AddRangeAsync(players, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
