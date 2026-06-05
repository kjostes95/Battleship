namespace Battleship.Domain;

public enum ShotOutcome
{
    Miss,
    Hit,
    Sunk
}

public sealed record ShotResult(ShotOutcome Outcome, string? ShipName = null);
