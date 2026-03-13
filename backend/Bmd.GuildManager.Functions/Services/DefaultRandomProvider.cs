using Bmd.GuildManager.Core.Abstractions;

namespace Bmd.GuildManager.Functions.Services;

public class DefaultRandomProvider : IRandomProvider
{
    public double NextDouble() =>
        Random.Shared.NextDouble();

    public double NextDouble(double minValue, double maxValue) =>
        minValue + (Random.Shared.NextDouble() * (maxValue - minValue));

    public int NextInt(int minValue, int maxValueExclusive) =>
        Random.Shared.Next(minValue, maxValueExclusive);
}
