using Course07_ResultAndExceptions.Common;
using Course07_ResultAndExceptions.Models;

namespace Course07_ResultAndExceptions.Services;

public sealed class PlayerManager
{
    private readonly Dictionary<Guid, Player> _playersById = new();

    public IReadOnlyCollection<Player> Players => _playersById.Values;

    public Result AddPlayer(Player player)
    {
        if (string.IsNullOrWhiteSpace(player.Name))
        {
            return Result.Failure("玩家名称不能为空");
        }

        bool nameExists = _playersById.Values.Any(p =>
            string.Equals(p.Name, player.Name, StringComparison.OrdinalIgnoreCase));

        if (nameExists)
        {
            return Result.Failure("玩家名称已存在");
        }

        return _playersById.TryAdd(player.Id, player)
            ? Result.Success()
            : Result.Failure("玩家 ID 已存在");
    }

    public Result<Player> GetPlayer(Guid id)
    {
        return _playersById.TryGetValue(id, out Player? player)
            ? Result<Player>.Success(player)
            : Result<Player>.Failure("玩家不存在");
    }
}
