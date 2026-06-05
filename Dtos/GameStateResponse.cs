namespace Battleship.Dtos;

public sealed class GameStateResponse
{
    public string GameId { get; init; } = null!;
    public int BoardSize { get; init; }
    public int ShotsFired { get; init; }
    public int ShipsRemaining { get; init; }
    public bool IsWon { get; init; }
    public IReadOnlyList<ShotHistoryItem> Shots { get; init; } = Array.Empty<ShotHistoryItem>();
}
