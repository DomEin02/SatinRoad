using API.Dtos;
using Infa;
using LinqToDB;

namespace Tests;

public class BuyFbiRaidTests : ApiTest
{
    private void InsertUser(string id)
    {
        Db.Insert(new User
        {
            Id = id,
            Username = id,
            PasswordHash = "not-used-here",
            Role = UserRole.User,
            IsShutDown = false,
            CreatedAtUtc = DateTime.UtcNow
        });
    }

    private Product InsertProduct(string vendorId)
    {
        var product = new Product
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test product",
            Description = "For testing",
            PriceDkk = 100m,
            StockCount = 50,
            CategoryId = "c1",
            VendorId = vendorId,
            CreatedAtUtc = DateTime.UtcNow
        };
        Db.Insert(product);
        return product;
    }

    [Fact]
    public void Nothing_happens_to_the_vendor_without_a_raid()
    {
        InsertUser("vendor");
        var product = InsertProduct("vendor");
        LoginAs("buyer");

        var order = OrdersController.Buy(new BuyRequest(product.Id, 1));

        Assert.False(order.VendorWasRaided);
        Assert.False(Db.Users().Single(u => u.Id == "vendor").IsShutDown);
        Assert.Single(Db.Products().ToList());
    }

    [Fact]
    public void A_raid_shuts_the_vendor_down()
    {
        InsertUser("vendor");
        var product = InsertProduct("vendor");
        LoginAs("buyer");
        RandomProvider.NextValue = 0.0;

        OrdersController.Buy(new BuyRequest(product.Id, 1));

        Assert.True(Db.Users().Single(u => u.Id == "vendor").IsShutDown);
    }

    [Fact]
    public void A_raid_removes_all_the_vendors_products()
    {
        InsertUser("vendor");
        var product = InsertProduct("vendor");
        InsertProduct("vendor");
        InsertProduct("vendor");
        LoginAs("buyer");
        RandomProvider.NextValue = 0.0;

        OrdersController.Buy(new BuyRequest(product.Id, 1));

        Assert.Empty(Db.Products().ToList());
    }

    [Fact]
    public void A_raid_leaves_other_vendors_alone()
    {
        InsertUser("vendor");
        InsertUser("other-vendor");
        var product = InsertProduct("vendor");
        InsertProduct("other-vendor");
        LoginAs("buyer");
        RandomProvider.NextValue = 0.0;

        OrdersController.Buy(new BuyRequest(product.Id, 1));

        Assert.False(Db.Users().Single(u => u.Id == "other-vendor").IsShutDown);
        Assert.Single(Db.Products().Where(p => p.VendorId == "other-vendor").ToList());
    }

    [Fact]
    public void The_order_is_still_saved_and_reports_the_raid()
    {
        InsertUser("vendor");
        var product = InsertProduct("vendor");
        LoginAs("buyer");
        RandomProvider.NextValue = 0.0;

        var order = OrdersController.Buy(new BuyRequest(product.Id, 1));

        Assert.True(order.VendorWasRaided);
        Assert.Single(Db.Orders().ToList());
    }
}