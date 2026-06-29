using Course05_LinqAndDtos.Dtos;
using Course05_LinqAndDtos.Models;

namespace Course05_LinqAndDtos.Services;

public sealed class PlayerQueryService
{
    public List<RankingPlayerDto> GetRanking(IEnumerable<Player> players, int count)
    {
        return players
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CalculatePower())
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

    public List<RegionStatDto> GetRegionStats(IEnumerable<Player> players)
    {
        return players
            .GroupBy(p => p.Region)
            .Select(g => new RegionStatDto
            {
                Region = g.Key,
                RegionName = Player.GetRegionName(g.Key),
                PlayerCount = g.Count(),
                ActivePlayerCount = g.Count(p => p.IsActive)
            })
            .OrderByDescending(x => x.PlayerCount)
            .ToList();
    }
}
