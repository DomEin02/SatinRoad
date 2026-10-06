namespace API.Services;

public static class DiscountCalculator
{
    public const int OrdersNeeded = 10;
    public const decimal DiscountRate = 0.20m;

    // After every 10 full-price orders with a vendor, the next order gets the discount:
    public static bool QualifiesForDiscount(int previousOrders)
    {
        return previousOrders % (OrdersNeeded + 1) == OrdersNeeded;
    }

    public static decimal TotalPrice(decimal unitPrice, int quantity, bool discountApplies)
    {
        var total = unitPrice * quantity;
        if (!discountApplies)
            return total;

        return Math.Round(total * (1 - DiscountRate), 2, MidpointRounding.AwayFromZero);
    }
}