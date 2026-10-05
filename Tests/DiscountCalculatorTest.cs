using API.Services;

namespace Tests;

public class DiscountCalculatorTest
{
    // [Theory] runs the same test once for every [InlineData] line.
    // The first number is how many orders the buyer already has with the vendor.
    [Theory]
    [InlineData(0, false)]  // first order ever
    [InlineData(9, false)]  // 10th order: still full price
    [InlineData(10, true)]  // 11th order: discount
    [InlineData(11, false)] // 12th order: back to full price
    [InlineData(20, false)] // 21st order: full price
    [InlineData(21, true)]  // 22nd order: discount again
    [InlineData(32, true)]  // 33rd order: discount again
    public void Every_11th_order_gets_the_discount(int previousOrders, bool expected)
    {
        Assert.Equal(expected, DiscountCalculator.QualifiesForDiscount(previousOrders));
    }

    [Fact]
    public void Charges_full_price_without_discount()
    {
        Assert.Equal(300m, DiscountCalculator.TotalPrice(100m, 3, false));
    }

    [Fact]
    public void Takes_20_percent_off_the_whole_order()
    {
        Assert.Equal(240m, DiscountCalculator.TotalPrice(100m, 3, true));
    }

    [Fact]
    public void Rounds_to_two_decimals()
    {
        // 9.99 * 0.80 = 7.992, which is not a real amount of money
        Assert.Equal(7.99m, DiscountCalculator.TotalPrice(9.99m, 1, true));
    }
}