namespace API.Dtos;

public class FeaturedVendorResponse
{
    public string VendorId { get; }
    public string Username { get; }
    public int OrderCount { get; }

    public FeaturedVendorResponse(string vendorId, string username, int orderCount)
    {
        VendorId = vendorId;
        Username = username;
        OrderCount = orderCount;
    }
}