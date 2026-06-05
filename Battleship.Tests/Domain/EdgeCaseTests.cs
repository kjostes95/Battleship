using Battleship.Domain;
using Xunit;

namespace Battleship.Tests.Domain;

public class EdgeCaseTests
{
    [Fact]
    public void Fire_OutOfBounds_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var rng = new MockRandomProvider();
        var board = new Board(10, rng);
        var outOfBoundsPosition = new BoardPosition(10, 10); // Size is 10, so valid range is 0-9

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => board.Fire(outOfBoundsPosition));
    }

    [Fact]
    public void Fire_OutOfBoundsNegative_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var rng = new MockRandomProvider();
        var board = new Board(10, rng);
        var outOfBoundsPosition = new BoardPosition(-1, 5);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => board.Fire(outOfBoundsPosition));
    }

    [Fact]
    public void Fire_DuplicateShot_ReturnsSameResult()
    {
        // Arrange
        var rng = new MockRandomProvider();
        var board = new Board(10, rng);
        var position = new BoardPosition(0, 0);

        // Act
        var firstResult = board.Fire(position);
        var secondResult = board.Fire(position);

        // Assert
        Assert.Equal(firstResult.Outcome, secondResult.Outcome);
        Assert.Equal(firstResult.ShipName, secondResult.ShipName);
    }

    [Fact]
    public void Fire_DuplicateShot_DoesNotIncrementShotsFired()
    {
        // Arrange
        var rng = new MockRandomProvider();
        var board = new Board(10, rng);
        var position = new BoardPosition(0, 0);

        // Act
        board.Fire(position);
        var shotsAfterFirst = board.ShotsFired;
        board.Fire(position);
        var shotsAfterSecond = board.ShotsFired;

        // Assert
        Assert.Equal(shotsAfterFirst, shotsAfterSecond);
    }

    [Fact]
    public void Fire_AllPositionsAtBoundary_WithinBounds()
    {
        // Arrange
        var rng = new MockRandomProvider();
        var boardSize = 10;
        var board = new Board(boardSize, rng);

        // Act & Assert - fire at all boundary positions (should not throw)
        for (int i = 0; i < boardSize; i++)
        {
            // Top edge
            var result1 = board.Fire(new BoardPosition(i, 0));
            Assert.NotNull(result1);
            
            // Bottom edge
            var result2 = board.Fire(new BoardPosition(i, boardSize - 1));
            Assert.NotNull(result2);
            
            // Left edge
            var result3 = board.Fire(new BoardPosition(0, i));
            Assert.NotNull(result3);
            
            // Right edge
            var result4 = board.Fire(new BoardPosition(boardSize - 1, i));
            Assert.NotNull(result4);
        }
    }

    [Fact]
    public void IsInBounds_ValidatesCorrectly()
    {
        // Arrange
        var rng = new MockRandomProvider();
        var board = new Board(10, rng);

        // Act & Assert
        Assert.True(board.IsInBounds(new BoardPosition(0, 0)));
        Assert.True(board.IsInBounds(new BoardPosition(9, 9)));
        Assert.True(board.IsInBounds(new BoardPosition(5, 5)));
        
        Assert.False(board.IsInBounds(new BoardPosition(10, 0)));
        Assert.False(board.IsInBounds(new BoardPosition(0, 10)));
        Assert.False(board.IsInBounds(new BoardPosition(-1, 5)));
        Assert.False(board.IsInBounds(new BoardPosition(5, -1)));
    }
}
