using Course15_EfCoreSqlite.Models;
using Microsoft.EntityFrameworkCore;

namespace Course15_EfCoreSqlite.Persistence;

public sealed class PlayerDbContext : DbContext
{
    public PlayerDbContext(DbContextOptions<PlayerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Player> Players => Set<Player>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>(entity =>
        {
            entity.ToTable("Players");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(p => p.Region)
                .HasMaxLength(16)
                .IsRequired();
        });
    }
}
