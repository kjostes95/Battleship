namespace Battleship.Domain;

public sealed class RandomProvider : IRandomProvider
{
    private readonly Random _random = new();

    public int Next(int minValue, int maxValue) => _random.Next(minValue, maxValue);
    public int Next(int maxValue) => _random.Next(maxValue);
}
