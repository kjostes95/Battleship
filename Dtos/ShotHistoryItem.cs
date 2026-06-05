namespace Battleship.Dtos;

public sealed class ShotHistoryItem
{
    public int X { get; init; }
    public int Y { get; init; }
    public string Outcome { get; init; } = null!;
    public string? ShipName { get; init; }
}
