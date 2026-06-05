namespace Battleship.Domain;

public sealed class Ship
{
    private readonly HashSet<BoardPosition> _hits = new();

    public Ship(string name, int length, IReadOnlyList<BoardPosition> positions)
    {
        Name = name;
        Length = length;
        Positions = positions;
    }

    public string Name { get; }
    public int Length { get; }
    public IReadOnlyList<BoardPosition> Positions { get; }
    public bool IsSunk => _hits.Count >= Length;

    public bool Occupies(BoardPosition position) => Positions.Contains(position);

    public bool RegisterHit(BoardPosition position)
    {
        if (!Occupies(position))
        {
            return false;
        }

        _hits.Add(position);
        return true;
    }
}
