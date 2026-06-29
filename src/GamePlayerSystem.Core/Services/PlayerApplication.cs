using GamePlayerSystem.Core.Common;
using GamePlayerSystem.Core.Dtos;
using GamePlayerSystem.Core.Models;
using GamePlayerSystem.Core.Repositories;

namespace GamePlayerSystem.Core.Services;

public sealed class PlayerApplication
{
    private readonly IPlayerRepository _repository;

    public PlayerApplication(IPlayerRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PlayerSummaryDto>> AddPlayerAsync(
        CreatePlayerRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<PlayerSummaryDto>.Failure("玩家名称不能为空");
        }

        string playerName = request.Name.Trim();

        if (await _repository.ExistsByNameAsync(playerName, cancellationToken: cancellationToken))
        {
            return Result<PlayerSummaryDto>.Failure("玩家名称已存在");
        }

        Player player = new Player
        {
            Name = playerName,
            Level = request.Level,
            Region = NormalizeRegion(request.Region),
            Gold = request.Gold
        };

        await _repository.AddAsync(player, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result<PlayerSummaryDto>.Success(ToSummaryDto(player));
    }

    public async Task<List<PlayerSummaryDto>> GetPlayersAsync(
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        List<Player> players = await _repository.GetPagedAsync(pageNumber, pageSize, cancellationToken);
        return players.Select(ToSummaryDto).ToList();
    }

    public async Task<Result<PlayerSummaryDto>> GetPlayerAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        Player? player = await _repository.GetByIdAsync(id, cancellationToken);

        return player is not null
            ? Result<PlayerSummaryDto>.Success(ToSummaryDto(player))
            : Result<PlayerSummaryDto>.Failure("玩家不存在");
    }

    public async Task<Result> UpdatePlayerAsync(
        Guid id,
        UpdatePlayerRequest request,
        CancellationToken cancellationToken = default)
    {
        Player? player = await _repository.GetTrackedByIdAsync(id, cancellationToken);

        if (player is null)
        {
            return Result.Failure("玩家不存在，无法修改");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure("玩家名称不能为空");
        }

        string playerName = request.Name.Trim();
        bool duplicateName = await _repository.ExistsByNameAsync(playerName, id, cancellationToken);

        if (duplicateName)
        {
            return Result.Failure("玩家名称已存在");
        }

        player.Name = playerName;
        player.Level = request.Level;
        player.Region = NormalizeRegion(request.Region);
        player.Gold = request.Gold;

        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RemoveByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        Player? player = await _repository.GetTrackedByIdAsync(id, cancellationToken);

        if (player is null)
        {
            return Result.Failure("玩家不存在，无法删除");
        }

        _repository.Remove(player);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DisableByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        Player? player = await _repository.GetTrackedByIdAsync(id, cancellationToken);

        if (player is null)
        {
            return Result.Failure("玩家不存在，无法禁用");
        }

        if (!player.IsActive)
        {
            return Result.Failure("玩家已经是禁用状态");
        }

        player.IsActive = false;

        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<List<PlayerSummaryDto>> GetPlayersByRegionAsync(
        string region,
        CancellationToken cancellationToken = default)
    {
        string normalizedRegion = NormalizeRegion(region);
        List<Player> players = await _repository.GetByRegionAsync(normalizedRegion, cancellationToken);

        return players.Select(ToSummaryDto).ToList();
    }

    public async Task<List<PlayerSummaryDto>> SearchPlayersByNameAsync(
        string keyword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return new List<PlayerSummaryDto>();
        }

        List<Player> players = await _repository.SearchByNameAsync(keyword, cancellationToken);
        return players.Select(ToSummaryDto).ToList();
    }

    public async Task<List<RankingPlayerDto>> GetRankingAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        List<Player> players = await _repository.GetTopByPowerAsync(count, cancellationToken);

        return players
            .Select((player, index) => new RankingPlayerDto
            {
                Rank = index + 1,
                PlayerId = player.Id,
                Name = player.Name,
                RegionName = player.GetRegionName(),
                Level = player.Level,
                Power = player.CalculatePower()
            })
            .ToList();
    }

    public Task<List<RegionStatDto>> GetRegionStatsAsync(CancellationToken cancellationToken = default)
    {
        return _repository.GetRegionStatsAsync(cancellationToken);
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
