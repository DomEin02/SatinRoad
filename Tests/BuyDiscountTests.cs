using API.Dtos;
using Infa;
using LinqToDB;

namespace Tests;

public class BuyDiscountTests : ApiTest
{
    private Product InsertProduct(string vendorId, decimal price = 100m)
    {
        var product = new Product
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test product",
            Description = "For testing",
            PriceDkk = price,
            StockCount = 50,
            CategoryId = "c1",
            VendorId = vendorId,
            CreatedAtUtc = DateTime.UtcNow
        };
        Db.Insert(product);
        return product;
    }

    // Puts earlier orders straight into the database, as if they were bought before
    private void InsertPreviousOrders(string buyerId, string vendorId, int count)
    {
        for (var i = 0; i < count; i++)
            Db.Insert(new Order
            {
                Id = Guid.NewGuid().ToString(),
                BuyerId = buyerId,
                VendorId = vendorId,
                ProductId = "old-product",
                Quantity = 1,
                UnitPriceDkk = 100m,
                TotalPriceDkk = 100m,
                CreatedAtUtc = DateTime.UtcNow
            });
    }

    [Fact]
    public void The_10th_order_is_full_price()
    {
        var product = InsertProduct("vendor");
        InsertPreviousOrders("buyer", "vendor", 9);
        LoginAs("buyer");

        var order = OrdersController.Buy(new BuyRequest(product.Id, 1));

        Assert.False(order.DiscountApplied);
        Assert.Equal(100m, order.TotalPriceDkk);
    }

    [Fact]
    public void The_11th_order_is_20_percent_off()
    {
        var product = InsertProduct("vendor");
        InsertPreviousOrders("buyer", "vendor", 10);
        LoginAs("buyer");

        var order = OrdersController.Buy(new BuyRequest(product.Id, 1));

        Assert.True(order.DiscountApplied);
        Assert.Equal(80m, order.TotalPriceDkk);
        // The unit price stays the real price, only the total is reduced
        Assert.Equal(100m, order.UnitPriceDkk);
    }

    [Fact]
    public void The_12th_order_is_full_price_again()
    {
        var product = InsertProduct("vendor");
        InsertPreviousOrders("buyer", "vendor", 11);
        LoginAs("buyer");

        var order = OrdersController.Buy(new BuyRequest(product.Id, 1));

        Assert.False(order.DiscountApplied);
        Assert.Equal(100m, order.TotalPriceDkk);
    }

    [Fact]
    public void Orders_with_another_vendor_do_not_count()
    {
        var product = InsertProduct("vendor");
        InsertPreviousOrders("buyer", "other-vendor", 10);
        LoginAs("buyer");

        var order = OrdersController.Buy(new BuyRequest(product.Id, 1));

        Assert.False(order.DiscountApplied);
    }

    [Fact]
    public void Other_buyers_orders_do_not_count()
    {
        var product = InsertProduct("vendor");
        InsertPreviousOrders("someone-else", "vendor", 10);
        LoginAs("buyer");

        var order = OrdersController.Buy(new BuyRequest(product.Id, 1));

        Assert.False(order.DiscountApplied);
    }
}