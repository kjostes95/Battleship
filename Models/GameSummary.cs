namespace Battleship.Models;

public sealed class GameSummary
{
    public int Id { get; set; }
    public string GameId { get; set; } = null!;
    public int BoardSize { get; set; }
    public int ShipCount { get; set; }
    public int TotalShots { get; set; }
    public DateTime CompletedAt { get; set; }
}
