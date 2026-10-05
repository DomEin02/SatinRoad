namespace API.Services;

public static class DiscountCalculator
{
    public const int OrdersNeeded = 10;
    public const decimal DiscountRate = 0.20m;

    public static bool QualifiesForDiscount(int previousOrders)
    {
        throw new NotImplementedException();
    }

    public static decimal TotalPrice(decimal unitPrice, int quantity, bool discountApplies)
    {
        throw new NotImplementedException();
    }
}