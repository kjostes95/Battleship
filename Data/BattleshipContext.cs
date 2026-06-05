using Battleship.Models;
using Microsoft.EntityFrameworkCore;

namespace Battleship.Data;

public class BattleshipContext : DbContext
{
    public BattleshipContext(DbContextOptions<BattleshipContext> options)
        : base(options)
    {
    }

    public DbSet<GameSummary> GameSummaries => Set<GameSummary>();
}
