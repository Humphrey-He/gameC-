using Course11_DependencyInjection.Models;

namespace Course11_DependencyInjection.Services;

public interface IPlayerStorage
{
    Task SaveAsync(
        IEnumerable<Player> players,
        CancellationToken cancellationToken = default);

    Task<List<Player>> LoadAsync(
        CancellationToken cancellationToken = default);
}
