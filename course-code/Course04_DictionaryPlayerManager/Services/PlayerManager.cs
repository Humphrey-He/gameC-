using Course04_DictionaryPlayerManager.Models;

namespace Course04_DictionaryPlayerManager.Services;

public sealed class PlayerManager
{
    private readonly Dictionary<Guid, Player> _playersById = new();

    public IReadOnlyCollection<Player> Players => _playersById.Values;

    public bool AddPlayer(Player player)
    {
        // TryAdd 可以避免重复 key 直接抛异常。
        return _playersById.TryAdd(player.Id, player);
    }

    public Player? FindById(Guid id)
    {
        return _playersById.TryGetValue(id, out Player? player)
            ? player
            : null;
    }

    public bool RemoveById(Guid id)
    {
        return _playersById.Remove(id);
    }

    public List<Player> GetTopByPower(int count)
    {
        return _playersById.Values
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CalculatePower())
            .Take(count)
            .ToList();
    }
}
