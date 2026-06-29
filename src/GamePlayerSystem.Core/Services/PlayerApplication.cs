using GamePlayerSystem.Core.Common;
using GamePlayerSystem.Core.Dtos;
using GamePlayerSystem.Core.Models;

namespace GamePlayerSystem.Core.Services;

public sealed class PlayerApplication
{
    private readonly Dictionary<Guid, Player> _playersById = new();

    public IReadOnlyCollection<Player> Players => _playersById.Values;

    public Result<PlayerSummaryDto> AddPlayer(CreatePlayerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<PlayerSummaryDto>.Failure("玩家名称不能为空");
        }

        if (NameExists(request.Name))
        {
            return Result<PlayerSummaryDto>.Failure("玩家名称已存在");
        }

        Player player = new Player
        {
            Name = request.Name.Trim(),
            Level = request.Level,
            Region = NormalizeRegion(request.Region),
            Gold = request.Gold
        };

        _playersById.Add(player.Id, player);

        return Result<PlayerSummaryDto>.Success(ToSummaryDto(player));
    }

    public List<PlayerSummaryDto> GetPlayers()
    {
        return _playersById.Values
            .OrderByDescending(p => p.Level)
            .ThenBy(p => p.Id)
            .Select(ToSummaryDto)
            .ToList();
    }

    public Result<PlayerSummaryDto> GetPlayer(Guid id)
    {
        return _playersById.TryGetValue(id, out Player? player)
            ? Result<PlayerSummaryDto>.Success(ToSummaryDto(player))
            : Result<PlayerSummaryDto>.Failure("玩家不存在");
    }

    public Result UpdatePlayer(Guid id, UpdatePlayerRequest request)
    {
        if (!_playersById.TryGetValue(id, out Player? player))
        {
            return Result.Failure("玩家不存在，无法修改");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure("玩家名称不能为空");
        }

        bool duplicateName = _playersById.Values.Any(p =>
            p.Id != id && string.Equals(p.Name, request.Name, StringComparison.OrdinalIgnoreCase));

        if (duplicateName)
        {
            return Result.Failure("玩家名称已存在");
        }

        player.Name = request.Name.Trim();
        player.Level = request.Level;
        player.Region = NormalizeRegion(request.Region);
        player.Gold = request.Gold;

        return Result.Success();
    }

    public Result RemoveById(Guid id)
    {
        return _playersById.Remove(id)
            ? Result.Success()
            : Result.Failure("玩家不存在，无法删除");
    }

    public Result DisableById(Guid id)
    {
        if (!_playersById.TryGetValue(id, out Player? player))
        {
            return Result.Failure("玩家不存在，无法禁用");
        }

        if (!player.IsActive)
        {
            return Result.Failure("玩家已经是禁用状态");
        }

        player.IsActive = false;

        return Result.Success();
    }

    public List<PlayerSummaryDto> GetPlayersByRegion(string region)
    {
        string normalizedRegion = NormalizeRegion(region);

        return _playersById.Values
            .Where(p => p.IsActive && p.Region == normalizedRegion)
            .OrderByDescending(p => p.Level)
            .ThenBy(p => p.Id)
            .Select(ToSummaryDto)
            .ToList();
    }

    public List<PlayerSummaryDto> SearchPlayersByName(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return new List<PlayerSummaryDto>();
        }

        return _playersById.Values
            .Where(p => p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(p => p.Level)
            .ThenBy(p => p.Id)
            .Select(ToSummaryDto)
            .ToList();
    }

    public List<RankingPlayerDto> GetRanking(int count)
    {
        return _playersById.Values
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CalculatePower())
            .ThenBy(p => p.Id)
            .Take(count)
            .Select((p, index) => new RankingPlayerDto
            {
                Rank = index + 1,
                PlayerId = p.Id,
                Name = p.Name,
                RegionName = p.GetRegionName(),
                Level = p.Level,
                Power = p.CalculatePower()
            })
            .ToList();
    }

    public List<RegionStatDto> GetRegionStats()
    {
        return _playersById.Values
            .GroupBy(p => p.Region)
            .Select(g => new RegionStatDto
            {
                Region = g.Key,
                RegionName = Player.GetRegionName(g.Key),
                PlayerCount = g.Count(),
                ActivePlayerCount = g.Count(p => p.IsActive)
            })
            .OrderByDescending(x => x.PlayerCount)
            .ThenBy(x => x.Region)
            .ToList();
    }

    private bool NameExists(string name)
    {
        return _playersById.Values.Any(p =>
            string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    private static PlayerSummaryDto ToSummaryDto(Player player)
    {
        return new PlayerSummaryDto
        {
            PlayerId = player.Id,
            Name = player.Name,
            Region = player.Region,
            RegionName = player.GetRegionName(),
            Level = player.Level,
            Gold = player.Gold,
            Power = player.CalculatePower(),
            IsActive = player.IsActive
        };
    }

    private static string NormalizeRegion(string region)
    {
        return string.IsNullOrWhiteSpace(region)
            ? "CN"
            : region.Trim().ToUpperInvariant();
    }
}
