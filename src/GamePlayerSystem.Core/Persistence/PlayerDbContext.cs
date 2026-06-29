using GamePlayerSystem.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GamePlayerSystem.Core.Persistence;

public sealed class PlayerDbContext : DbContext
{
    public PlayerDbContext(DbContextOptions<PlayerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Player> Players => Set<Player>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Player>(entity =>
        {
            entity.ToTable("Players");
            entity.HasKey(player => player.Id);

            entity.Property(player => player.Name)
                .HasMaxLength(64)
                .UseCollation("NOCASE")
                .IsRequired();

            entity.Property(player => player.Region)
                .HasMaxLength(16)
                .IsRequired();

            entity.HasIndex(player => player.Name)
                .IsUnique();

            entity.HasIndex(player => player.Region);
        });
    }
}
