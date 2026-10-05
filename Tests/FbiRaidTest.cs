using API.Services;

namespace Tests;

public class FbiRaidTest
{
    // The roll is the random number. A roll below 0.01 means the buyer is FBI.
    [Theory]
    [InlineData(0.0, true)]    // lowest possible roll
    [InlineData(0.009, true)]  // just under the line
    [InlineData(0.01, false)]  // exactly on the line is not a raid
    [InlineData(0.5, false)]
    [InlineData(0.99, false)]
    public void Only_a_roll_below_one_percent_is_a_raid(double roll, bool expected)
    {
        Assert.Equal(expected, FbiRaid.IsRaid(roll));
    }
}