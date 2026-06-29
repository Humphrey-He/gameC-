using Course11_DependencyInjection.Models;

namespace Course11_DependencyInjection.Services;

public sealed class PlayerApplication
{
    private readonly IPlayerStorage _storage;
    private readonly List<Player> _players = new();

    public PlayerApplication(IPlayerStorage storage)
    {
        _storage = storage;
    }

    public IReadOnlyList<Player> Players => _players;

    public void AddPlayer(Player player)
    {
        _players.Add(player);
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        _players.Clear();
        _players.AddRange(await _storage.LoadAsync(cancellationToken));
    }

    public Task SaveAsync(CancellationToken cancellationToken = default)
    {
        return _storage.SaveAsync(_players, cancellationToken);
    }
}
