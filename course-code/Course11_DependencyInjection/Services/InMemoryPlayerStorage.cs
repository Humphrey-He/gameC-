using Course11_DependencyInjection.Models;

namespace Course11_DependencyInjection.Services;

public sealed class InMemoryPlayerStorage : IPlayerStorage
{
    private readonly List<Player> _players = new();

    public Task SaveAsync(
        IEnumerable<Player> players,
        CancellationToken cancellationToken = default)
    {
        _players.Clear();
        _players.AddRange(players);

        return Task.CompletedTask;
    }

    public Task<List<Player>> LoadAsync(
        CancellationToken cancellationToken = default)
    {
        // 返回副本，避免调用方直接修改内部列表。
        return Task.FromResult(_players.ToList());
    }
}
