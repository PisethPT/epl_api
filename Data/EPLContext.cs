using epl_api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace epl_api.Data;

public class EPLContext : IdentityDbContext<User>
{
    public DbSet<Team> Teams { get; set; }
    public DbSet<Player> Players { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<Goal> Goals { get; set; }
    public DbSet<Assist> Assists { get; set; }
    public DbSet<Card> Cards { get; set; }
    public DbSet<News> News { get; set; }
    public DbSet<Season> Seasons { get; set; }
    public DbSet<MatchSeason> MatchSeasons { get; set; }

    public EPLContext(DbContextOptions<EPLContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Match ↔ Team
        modelBuilder.Entity<Match>()
            .HasOne(m => m.HomeTeam)
            .WithMany(t => t.HomeMatches)
            .HasForeignKey(m => m.HomeTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        // Match ↔ Team
        modelBuilder.Entity<Match>()
            .HasOne(m => m.AwayTeam)
            .WithMany(t => t.AwayMatches)
            .HasForeignKey(m => m.AwayTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        // Assist ↔ Match
        modelBuilder.Entity<Assist>()
            .HasOne(a => a.Match)
            .WithMany(m => m.Assists)
            .HasForeignKey(a => a.MatchId)
            .OnDelete(DeleteBehavior.Restrict);

        // Assist ↔ Player
        modelBuilder.Entity<Assist>()
            .HasOne(a => a.Player)
            .WithMany(p => p.Assists)
            .HasForeignKey(a => a.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Assist ↔ Team
        modelBuilder.Entity<Assist>()
            .HasOne(a => a.Team)
            .WithMany(t => t.Assists)
            .HasForeignKey(a => a.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        // Card ↔ Match
        modelBuilder.Entity<Card>()
            .HasOne(a => a.Match)
            .WithMany(m => m.Cards)
            .HasForeignKey(a => a.MatchId)
            .OnDelete(DeleteBehavior.Restrict);

        // Card ↔ Player
        modelBuilder.Entity<Card>()
            .HasOne(a => a.Player)
            .WithMany(p => p.Cards)
            .HasForeignKey(a => a.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Card ↔ Team
        modelBuilder.Entity<Card>()
            .HasOne(a => a.Team)
            .WithMany(t => t.Cards)
            .HasForeignKey(a => a.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        // Goal ↔ Match
        modelBuilder.Entity<Goal>()
            .HasOne(a => a.Match)
            .WithMany(m => m.Goals)
            .HasForeignKey(a => a.MatchId)
            .OnDelete(DeleteBehavior.Restrict);

        // Goal ↔ Player
        modelBuilder.Entity<Goal>()
            .HasOne(a => a.Player)
            .WithMany(p => p.Goals)
            .HasForeignKey(a => a.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Goal ↔ Team
        modelBuilder.Entity<Goal>()
            .HasOne(a => a.Team)
            .WithMany(t => t.Goals)
            .HasForeignKey(a => a.TeamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
