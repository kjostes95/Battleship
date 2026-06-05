using System.Text;

namespace Battleship.Domain;

public sealed class Board
{
    private readonly Dictionary<BoardPosition, ShotResult> _shotCache = new();
    private readonly List<Ship> _ships = new();

    public Board(int size, IRandomProvider rng)
    {
        Size = size;
        Random = rng;
        PlaceFleet();
    }

    public int Size { get; }
    public IReadOnlyList<Ship> Ships => _ships;
    public IReadOnlyDictionary<BoardPosition, ShotResult> Shots => _shotCache;
    private IRandomProvider Random { get; }

    public int ShipsRemaining => _ships.Count(s => !s.IsSunk);
    public int ShotsFired => _shotCache.Count;

    public bool IsInBounds(BoardPosition position) =>
        position.X >= 0 && position.X < Size && position.Y >= 0 && position.Y < Size;

    public ShotResult Fire(BoardPosition position)
    {
        if (!IsInBounds(position))
        {
            throw new ArgumentOutOfRangeException(nameof(position), "Shot is out of bounds.");
        }

        if (_shotCache.TryGetValue(position, out var existing))
        {
            return existing;
        }

        var ship = _ships.FirstOrDefault(s => s.Occupies(position));
        if (ship is null)
        {
            var missResult = new ShotResult(ShotOutcome.Miss);
            _shotCache[position] = missResult;
            return missResult;
        }

        ship.RegisterHit(position);
        var outcome = ship.IsSunk ? ShotOutcome.Sunk : ShotOutcome.Hit;
        var result = new ShotResult(outcome, ship.Name);
        _shotCache[position] = result;
        return result;
    }

    private void PlaceFleet()
    {
        var shipDefinitions = new[]
        {
            new { Name = "Carrier", Length = 5 },
            new { Name = "Battleship", Length = 4 },
            new { Name = "Cruiser", Length = 3 },
            new { Name = "Submarine", Length = 3 },
            new { Name = "Destroyer", Length = 2 }
        };

        foreach (var shipDefinition in shipDefinitions)
        {
            const int maxAttempts = 1000;
            var placed = false;

            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                var horizontal = Random.Next(2) == 0;
                var x = horizontal ? Random.Next(Size - shipDefinition.Length + 1) : Random.Next(Size);
                var y = horizontal ? Random.Next(Size) : Random.Next(Size - shipDefinition.Length + 1);

                var positions = new List<BoardPosition>();
                for (var offset = 0; offset < shipDefinition.Length; offset++)
                {
                    positions.Add(new BoardPosition(
                        x + (horizontal ? offset : 0),
                        y + (horizontal ? 0 : offset)));
                }

                if (positions.Any(IsOccupied))
                {
                    continue;
                }

                _ships.Add(new Ship(shipDefinition.Name, shipDefinition.Length, positions));
                placed = true;
                break;
            }

            if (!placed)
            {
                throw new InvalidOperationException("Unable to place the fleet without overlap.");
            }
        }
    }

    private bool IsOccupied(BoardPosition position) => _ships.Any(ship => ship.Occupies(position));
}
