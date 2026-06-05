namespace Battleship.Dtos;

public sealed class ShotResponse
{
    public string Outcome { get; init; } = null!;
    public string? ShipName { get; init; }
    public int ShotsFired { get; init; }
    public int ShipsRemaining { get; init; }
    public bool IsWon { get; init; }
}
