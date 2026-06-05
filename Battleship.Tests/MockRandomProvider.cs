using Battleship.Domain;

namespace Battleship.Tests;

/// <summary>
/// Mock implementation of IRandomProvider for deterministic testing.
/// </summary>
public class MockRandomProvider : IRandomProvider
{
    private readonly Random _random;

    public MockRandomProvider(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public int Next(int maxValue)
    {
        return _random.Next(maxValue);
    }

    public int Next(int minValue, int maxValue)
    {
        return _random.Next(minValue, maxValue);
    }
}
