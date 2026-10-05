using API.Services;

namespace Tests;

// A random provider that always returns the number the test chooses.
// The default 0.99 means "no raid", so other tests are not affected.
public class FakeRandomProvider : IRandomProvider
{
    public double NextValue { get; set; } = 0.99;

    public double NextDouble()
    {
        return NextValue;
    }
}