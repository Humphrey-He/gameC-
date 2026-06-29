using Course16_RepositoryEfCore.Models;
using Microsoft.EntityFrameworkCore;

namespace Course16_RepositoryEfCore.Persistence;

public sealed class PlayerDbContext : DbContext
{
    public PlayerDbContext(DbContextOptions<PlayerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Player> Players => Set<Player>();
}
