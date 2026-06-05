using Battleship.Domain;
using Xunit;

namespace Battleship.Tests.Domain;

public class GameWinConditionTests
{
    [Fact]
    public void Game_IsNotWon_Initially()
    {
        // Arrange & Act
        var rng = new MockRandomProvider();
        var game = new Game("test-game", 10, rng);

        // Assert
        Assert.False(game.IsWon);
    }

    [Fact]
    public void Game_IsWon_AfterAllShipsAreSunk()
    {
        // Arrange
        var rng = new MockRandomProvider();
        var game = new Game("test-game", 10, rng);

        // Act - sink all ships
        foreach (var ship in game.Board.Ships)
        {
            foreach (var position in ship.Positions)
            {
                game.Board.Fire(position);
            }
        }

        // Assert
        Assert.True(game.IsWon);
    }

    [Fact]
    public void Game_NotWon_WithOneShipRemaining()
    {
        // Arrange
        var rng = new MockRandomProvider();
        var game = new Game("test-game", 10, rng);

        // Act - sink all ships except the first one
        foreach (var ship in game.Board.Ships.Skip(1))
        {
            foreach (var position in ship.Positions)
            {
                game.Board.Fire(position);
            }
        }

        // Assert
        Assert.False(game.IsWon);
        Assert.Equal(1, game.Board.ShipsRemaining);
    }

    [Fact]
    public void Game_ShotAfterWin_StillAllowsShots()
    {
        // Arrange
        var rng = new MockRandomProvider();
        var game = new Game("test-game", 10, rng);

        // Sink all ships
        foreach (var ship in game.Board.Ships)
        {
            foreach (var position in ship.Positions)
            {
                game.Board.Fire(position);
            }
        }

        Assert.True(game.IsWon);

        // Act - fire a shot after winning
        var emptyPosition = FindEmptyPosition(game.Board);
        var result = game.Board.Fire(emptyPosition);

        // Assert - should still work
        Assert.Equal(ShotOutcome.Miss, result.Outcome);
    }

    [Fact]
    public void Game_CreatedAtIsSet()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        var rng = new MockRandomProvider();

        // Act
        var game = new Game("test-game", 10, rng);
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.InRange(game.CreatedAt, beforeCreation.AddSeconds(-1), afterCreation.AddSeconds(1));
    }

    [Fact]
    public void Game_IdIsPreserved()
    {
        // Arrange
        var gameId = "test-game-123";
        var rng = new MockRandomProvider();

        // Act
        var game = new Game(gameId, 10, rng);

        // Assert
        Assert.Equal(gameId, game.Id);
    }

    private static BoardPosition FindEmptyPosition(Board board)
    {
        for (int x = 0; x < board.Size; x++)
        {
            for (int y = 0; y < board.Size; y++)
            {
                var pos = new BoardPosition(x, y);
                if (!board.Ships.Any(s => s.Occupies(pos)) && !board.Shots.ContainsKey(pos))
                {
                    return pos;
                }
            }
        }
        throw new InvalidOperationException("No empty positions available");
    }
}
