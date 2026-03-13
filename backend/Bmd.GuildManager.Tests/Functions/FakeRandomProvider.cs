using Bmd.GuildManager.Core.Abstractions;

namespace Bmd.GuildManager.Tests.Functions;

/// <summary>
/// A deterministic IRandomProvider for unit tests.
/// NextDouble() returns values from a fixed sequence, cycling if exhausted.
/// NextInt() returns the midpoint of the provided range.
/// </summary>
public class FakeRandomProvider(params double[] sequence) : IRandomProvider
{
    private int _index = 0;

    public double NextDouble()
    {
        var value = sequence[_index % sequence.Length];
        _index++;
        return value;
    }

    public double NextDouble(double minValue, double maxValue)
    {
        var t = NextDouble(); // 0.0–1.0
        return minValue + (t * (maxValue - minValue));
    }

    public int NextInt(int minValue, int maxValueExclusive) =>
        (minValue + maxValueExclusive) / 2;
}
