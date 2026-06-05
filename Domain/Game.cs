namespace Battleship.Domain;

public sealed class Game
{
    public Game(string gameId, int boardSize, IRandomProvider randomProvider)
    {
        Id = gameId;
        Board = new Board(boardSize, randomProvider);
        CreatedAt = DateTime.UtcNow;
    }

    public string Id { get; }
    public Board Board { get; }
    public DateTime CreatedAt { get; }
    public bool IsWon => Board.ShipsRemaining == 0;
}
