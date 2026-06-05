using Battleship.Data;
using Battleship.Domain;
using Battleship.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Battleship.Tests.Domain;

public class GamePersistenceTests
{
    private BattleshipContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BattleshipContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new BattleshipContext(options);
    }

    [Fact]
    public void GameSummary_CanBeSavedAndRetrieved()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var gameId = "completed-game-1";
        var summary = new GameSummary
        {
            GameId = gameId,
            BoardSize = 10,
            ShipCount = 5,
            TotalShots = 42,
            CompletedAt = DateTime.UtcNow
        };

        // Act
        context.GameSummaries.Add(summary);
        context.SaveChanges();

        // Assert
        var retrieved = context.GameSummaries.First(g => g.GameId == gameId);
        Assert.Equal(gameId, retrieved.GameId);
        Assert.Equal(10, retrieved.BoardSize);
        Assert.Equal(5, retrieved.ShipCount);
        Assert.Equal(42, retrieved.TotalShots);
    }

    [Fact]
    public void CompletedGame_PersistsWithCorrectShotCount()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var rng = new MockRandomProvider();
        var game = new Game("completed-game-2", 10, rng);
        var shotCount = 0;

        // Act - complete the game and count shots
        foreach (var ship in game.Board.Ships)
        {
            foreach (var position in ship.Positions)
            {
                game.Board.Fire(position);
                shotCount++;
            }
        }

        // Create and save the summary
        var summary = new GameSummary
        {
            GameId = game.Id,
            BoardSize = 10,
            ShipCount = game.Board.Ships.Count,
            TotalShots = game.Board.ShotsFired,
            CompletedAt = DateTime.UtcNow
        };

        context.GameSummaries.Add(summary);
        context.SaveChanges();

        // Assert
        Assert.True(game.IsWon);
        Assert.Equal(shotCount, game.Board.ShotsFired);
        
        var retrieved = context.GameSummaries.First(g => g.GameId == game.Id);
        Assert.Equal(shotCount, retrieved.TotalShots);
        Assert.Equal(game.Board.ShotsFired, retrieved.TotalShots);
    }

    [Fact]
    public void GameSummary_TotalShots_MatchesBoardShotsFired()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var rng = new MockRandomProvider();
        var game = new Game("test-game", 10, rng);
        
        // Fire some shots
        var ship = game.Board.Ships.First();
        var positions = ship.Positions.Take(3).ToList();
        foreach (var pos in positions)
        {
            game.Board.Fire(pos);
        }

        // Act
        var summary = new GameSummary
        {
            GameId = game.Id,
            BoardSize = 10,
            ShipCount = game.Board.Ships.Count,
            TotalShots = game.Board.ShotsFired,
            CompletedAt = DateTime.UtcNow
        };

        context.GameSummaries.Add(summary);
        context.SaveChanges();

        // Assert
        Assert.Equal(3, game.Board.ShotsFired);
        var retrieved = context.GameSummaries.First();
        Assert.Equal(3, retrieved.TotalShots);
    }

    [Fact]
    public void MultiplGameSummaries_CanBeSavedAndRetrieved()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act
        for (int i = 1; i <= 3; i++)
        {
            var summary = new GameSummary
            {
                GameId = $"game-{i}",
                BoardSize = 10,
                ShipCount = 5,
                TotalShots = 20 + i,
                CompletedAt = DateTime.UtcNow
            };
            context.GameSummaries.Add(summary);
        }
        context.SaveChanges();

        // Assert
        var allSummaries = context.GameSummaries.ToList();
        Assert.Equal(3, allSummaries.Count);
        Assert.Equal(21, allSummaries.First(g => g.GameId == "game-1").TotalShots);
        Assert.Equal(22, allSummaries.First(g => g.GameId == "game-2").TotalShots);
        Assert.Equal(23, allSummaries.First(g => g.GameId == "game-3").TotalShots);
    }
}
