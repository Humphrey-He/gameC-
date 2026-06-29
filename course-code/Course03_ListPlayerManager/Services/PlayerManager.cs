using Course03_ListPlayerManager.Models;

namespace Course03_ListPlayerManager.Services;

public sealed class PlayerManager
{
    private readonly List<Player> _players = new();

    public IReadOnlyList<Player> Players => _players;

    public void AddPlayer(Player player)
    {
        _players.Add(player);
    }

    public Player? FindById(Guid id)
    {
        return _players.FirstOrDefault(p => p.Id == id);
    }

    public List<Player> FindByName(string keyword)
    {
        return _players
            .Where(p => p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public bool RemoveById(Guid id)
    {
        Player? player = FindById(id);
        return player is not null && _players.Remove(player);
    }

    public List<Player> GetTopByPower(int count)
    {
        return _players
            .OrderByDescending(p => p.CalculatePower())
            .Take(count)
            .ToList();
    }
}
