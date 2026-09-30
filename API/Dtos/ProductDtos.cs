namespace API.Dtos;

public record ProductCreateRequest(
    string Name,
    string Description,
    decimal PriceDkk,
    int StockCount,
    string CategoryId,
    string VendorId);

public record ProductUpdateRequest(
    string Id,
    string Name,
    string Description,
    decimal PriceDkk,
    int StockCount,
    string CategoryId);

public class ProductResponse
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public decimal PriceDkk { get; }
    public int StockCount { get; }
    public string CategoryId { get; }
    public string VendorId { get; }
    public DateTime CreatedAtUtc { get; }

    public ProductResponse(Infa.Product product)
    {
        Id = product.Id;
        Name = product.Name;
        Description = product.Description;
        PriceDkk = product.PriceDkk;
        StockCount = product.StockCount;
        CategoryId = product.CategoryId;
        VendorId = product.VendorId;
        CreatedAtUtc = product.CreatedAtUtc;
    }
}
    