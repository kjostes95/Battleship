using Battleship.Domain;
using Xunit;

namespace Battleship.Tests.Domain;

public class BoardPlacementTests
{
    [Fact]
    public void PlaceFleet_AllShipsPlacedWithinBounds()
    {
        // Arrange
        var rng = new MockRandomProvider();
        var boardSize = 10;

        // Act
        var board = new Board(boardSize, rng);

        // Assert
        Assert.NotEmpty(board.Ships);
        foreach (var ship in board.Ships)
        {
            foreach (var position in ship.Positions)
            {
                Assert.True(board.IsInBounds(position), $"Ship {ship.Name} position ({position.X}, {position.Y}) is out of bounds");
            }
        }
    }

    [Fact]
    public void PlaceFleet_NoShipsOverlap()
    {
        // Arrange
        var rng = new MockRandomProvider();
        var boardSize = 10;

        // Act
        var board = new Board(boardSize, rng);

        // Assert
        var allPositions = new HashSet<BoardPosition>();
        foreach (var ship in board.Ships)
        {
            foreach (var position in ship.Positions)
            {
                Assert.True(allPositions.Add(position), $"Position ({position.X}, {position.Y}) is occupied by multiple ships");
            }
        }
    }

    [Fact]
    public void PlaceFleet_DeterministicWithSameSeed()
    {
        // Arrange
        var rng1 = new MockRandomProvider(seed: 42);
        var rng2 = new MockRandomProvider(seed: 42);
        var boardSize = 10;

        // Act
        var board1 = new Board(boardSize, rng1);
        var board2 = new Board(boardSize, rng2);

        // Assert
        Assert.Equal(board1.Ships.Count, board2.Ships.Count);
        for (int i = 0; i < board1.Ships.Count; i++)
        {
            Assert.Equal(board1.Ships[i].Name, board2.Ships[i].Name);
            Assert.Equal(board1.Ships[i].Length, board2.Ships[i].Length);
            Assert.Equal(board1.Ships[i].Positions, board2.Ships[i].Positions);
        }
    }

    [Fact]
    public void PlaceFleet_CorrectNumberOfShips()
    {
        // Arrange
        var rng = new MockRandomProvider();

        // Act
        var board = new Board(10, rng);

        // Assert
        Assert.Equal(5, board.Ships.Count);
        var shipNames = board.Ships.Select(s => s.Name).ToList();
        Assert.Contains("Carrier", shipNames);
        Assert.Contains("Battleship", shipNames);
        Assert.Contains("Cruiser", shipNames);
        Assert.Contains("Submarine", shipNames);
        Assert.Contains("Destroyer", shipNames);
    }

    [Fact]
    public void PlaceFleet_CorrectShipLengths()
    {
        // Arrange
        var rng = new MockRandomProvider();
        var board = new Board(10, rng);

        // Act & Assert
        var carrier = board.Ships.First(s => s.Name == "Carrier");
        var battleship = board.Ships.First(s => s.Name == "Battleship");
        var cruiser = board.Ships.First(s => s.Name == "Cruiser");
        var submarine = board.Ships.First(s => s.Name == "Submarine");
        var destroyer = board.Ships.First(s => s.Name == "Destroyer");

        Assert.Equal(5, carrier.Length);
        Assert.Equal(4, battleship.Length);
        Assert.Equal(3, cruiser.Length);
        Assert.Equal(3, submarine.Length);
        Assert.Equal(2, destroyer.Length);
    }
}
