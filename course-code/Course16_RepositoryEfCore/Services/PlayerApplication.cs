using Course16_RepositoryEfCore.Common;
using Course16_RepositoryEfCore.Dtos;
using Course16_RepositoryEfCore.Models;
using Course16_RepositoryEfCore.Repositories;

namespace Course16_RepositoryEfCore.Services;

public sealed class PlayerApplication
{
    private readonly IPlayerRepository _repository;

    public PlayerApplication(IPlayerRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Player>> AddPlayerAsync(
        Player player,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(player.Name))
        {
            return Result<Player>.Failure("玩家名称不能为空");
        }

        if (await _repository.ExistsByNameAsync(player.Name, cancellationToken))
        {
            return Result<Player>.Failure("玩家名称已存在");
        }

        await _repository.AddAsync(player, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result<Player>.Success(player);
    }

    public async Task<PagedResult<PlayerSummaryDto>> GetPlayersAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        List<Player> players = await _repository.GetPagedAsync(
            pageNumber,
            pageSize,
            cancellationToken);

        return new PagedResult<PlayerSummaryDto>
        {
            PageNumber = Math.Max(pageNumber, 1),
            PageSize = Math.Clamp(pageSize, 1, 100),
            Items = players.Select(ToSummaryDto).ToList()
        };
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

        player.IsActive = false;
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static PlayerSummaryDto ToSummaryDto(Player player)
    {
        return new PlayerSummaryDto
        {
            PlayerId = player.Id,
            Name = player.Name,
            Region = player.Region,
            Level = player.Level,
            IsActive = player.IsActive
        };
    }
}
