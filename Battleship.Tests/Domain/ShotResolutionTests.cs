using Battleship.Domain;
using Xunit;

namespace Battleship.Tests.Domain;

public class ShotResolutionTests
{
    [Fact]
    public void Fire_Miss_WhenHittingEmptySpace()
    {
        // Arrange
        var rng = new MockRandomProvider();
        var board = new Board(10, rng);
        var emptyPosition = new BoardPosition(0, 0);
        
        // Ensure position is not occupied
        while (board.Ships.Any(s => s.Occupies(emptyPosition)))
        {
            emptyPosition = new BoardPosition(emptyPosition.X + 1, emptyPosition.Y);
        }

        // Act
        var result = board.Fire(emptyPosition);

        // Assert
        Assert.Equal(ShotOutcome.Miss, result.Outcome);
        Assert.Null(result.ShipName);
    }

    [Fact]
    public void Fire_Hit_WhenHittingUnhitShip()
    {
        // Arrange
        var rng = new MockRandomProvider();
        var board = new Board(10, rng);
        var ship = board.Ships.First();
        var position = ship.Positions.First();

        // Act
        var result = board.Fire(position);

        // Assert
        Assert.Equal(ShotOutcome.Hit, result.Outcome);
        Assert.Equal(ship.Name, result.ShipName);
    }

    [Fact]
    public void Fire_Sunk_WhenAllPositionsOfShipAreHit()
    {
        // Arrange
        var rng = new MockRandomProvider();
        var board = new Board(10, rng);
        var destroyer = board.Ships.First(s => s.Name == "Destroyer"); // Length 2, easiest to sink
        var positions = destroyer.Positions.ToList();

        // Act - fire at all positions except the last
        for (int i = 0; i < positions.Count - 1; i++)
        {
            var result = board.Fire(positions[i]);
            Assert.Equal(ShotOutcome.Hit, result.Outcome);
        }

        // Fire at the last position
        var finalResult = board.Fire(positions.Last());

        // Assert
        Assert.Equal(ShotOutcome.Sunk, finalResult.Outcome);
        Assert.Equal(destroyer.Name, finalResult.ShipName);
    }

    [Fact]
    public void Fire_UpdatesShotsFired()
    {
        // Arrange
        var rng = new MockRandomProvider();
        var board = new Board(10, rng);
        var position1 = new BoardPosition(0, 0);
        var position2 = new BoardPosition(1, 0);

        // Act
        board.Fire(position1);
        board.Fire(position2);

        // Assert
        Assert.Equal(2, board.ShotsFired);
    }

    [Fact]
    public void Fire_DecrementsShipsRemaining_WhenShipIsSunk()
    {
        // Arrange
        var rng = new MockRandomProvider();
        var board = new Board(10, rng);
        var initialShipsRemaining = board.ShipsRemaining;
        var destroyer = board.Ships.First(s => s.Name == "Destroyer");
        var positions = destroyer.Positions.ToList();

        // Act
        foreach (var position in positions)
        {
            board.Fire(position);
        }

        // Assert
        Assert.Equal(initialShipsRemaining - 1, board.ShipsRemaining);
    }

    [Fact]
    public void Fire_AllOutcomesCaptureCorrectly()
    {
        // Arrange
        var rng = new MockRandomProvider();
        var board = new Board(10, rng);
        var hitOutcomes = new List<ShotOutcome>();
        var missOutcomes = new List<ShotOutcome>();
        var sunkOutcomes = new List<ShotOutcome>();

        // Act - fire shots and collect outcomes
        var emptyPositions = GetEmptyPositions(board).Take(5).ToList();
        foreach (var pos in emptyPositions)
        {
            missOutcomes.Add(board.Fire(pos).Outcome);
        }

        var ship = board.Ships.First(s => s.Name == "Destroyer");
        foreach (var pos in ship.Positions.SkipLast(1))
        {
            hitOutcomes.Add(board.Fire(pos).Outcome);
        }
        
        sunkOutcomes.Add(board.Fire(ship.Positions.Last()).Outcome);

        // Assert
        Assert.All(missOutcomes, outcome => Assert.Equal(ShotOutcome.Miss, outcome));
        Assert.All(hitOutcomes, outcome => Assert.Equal(ShotOutcome.Hit, outcome));
        Assert.All(sunkOutcomes, outcome => Assert.Equal(ShotOutcome.Sunk, outcome));
    }

    private static List<BoardPosition> GetEmptyPositions(Board board)
    {
        var emptyPositions = new List<BoardPosition>();
        for (int x = 0; x < board.Size; x++)
        {
            for (int y = 0; y < board.Size; y++)
            {
                var pos = new BoardPosition(x, y);
                if (!board.Ships.Any(s => s.Occupies(pos)))
                {
                    emptyPositions.Add(pos);
                }
            }
        }
        return emptyPositions;
    }
}
