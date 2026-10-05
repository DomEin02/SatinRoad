namespace API.Dtos;

public record BuyRequest(string ProductId, int Quantity);

public class OrderResponse
{
    public string Id { get; }
    public string BuyerId { get; }
    public string VendorId { get; }
    public string ProductId { get; }
    public int Quantity { get; }
    public decimal UnitPriceDkk { get; }
    public bool DiscountApplied { get; }
    public decimal TotalPriceDkk { get; }
    public DateTime CreatedAtUtc { get; }
    public bool VendorWasRaided { get; }

    public OrderResponse(Infa.Order order, bool vendorWasRaided = false)
    {
        Id = order.Id;
        BuyerId = order.BuyerId;
        VendorId = order.VendorId;
        ProductId = order.ProductId;
        Quantity = order.Quantity;
        UnitPriceDkk = order.UnitPriceDkk;
        DiscountApplied = order.DiscountApplied;
        TotalPriceDkk = order.TotalPriceDkk;
        CreatedAtUtc = order.CreatedAtUtc;
        VendorWasRaided = vendorWasRaided;
    }
}