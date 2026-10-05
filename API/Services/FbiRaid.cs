namespace API.Services;

// Random number from 0.0 - 1.0
public interface IRandomProvider
{
    double NextDouble();
}

// The real one, used when the app runs
public class SystemRandomProvider : IRandomProvider
{
    public double NextDouble()
    {
        return Random.Shared.NextDouble();
    }
}

public static class FbiRaid
{
    public const double Chance = 0.01;

    public static bool IsRaid(double roll)
    {
        return roll < Chance;
    }
}