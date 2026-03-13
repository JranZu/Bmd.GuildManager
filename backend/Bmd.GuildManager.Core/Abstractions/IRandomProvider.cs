namespace Bmd.GuildManager.Core.Abstractions;

/// <summary>
/// Abstracts random number generation to enable deterministic testing.
/// </summary>
public interface IRandomProvider
{
    double NextDouble();
    double NextDouble(double minValue, double maxValue);
    int NextInt(int minValue, int maxValueExclusive);
}
