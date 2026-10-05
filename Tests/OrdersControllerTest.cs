using System.ComponentModel.DataAnnotations;
using API.Dtos;
using Infa;
using LinqToDB;
using Xunit;

namespace Tests;

public class OrdersControllerTest
{
    // Inserts a product directly into the database, without going through the controller
    private static Product InsertProduct(SatinRoadDatabase db, string vendorId, decimal price = 100m, int stock = 5)
    {
        var product = new Product
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test product",
            Description = "For testing",
            PriceDkk = price,
            StockCount = stock,
            CategoryId = "c1",
            VendorId = vendorId,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Insert(product);
        return product;
    }

    #region Tests: Buy

    public class BuyTests : ApiTest
    {
        [Fact]
        public void Creates_an_order_and_lowers_the_stock()
        {
            var product = OrdersControllerTest.InsertProduct(Db, vendorId: "vendor", stock: 5);
            LoginAs("buyer");

            OrdersController.Buy(new BuyRequest(product.Id, 2));

            Assert.Single(Db.Orders().ToList());
            Assert.Equal(3, Db.Products().Single(p => p.Id == product.Id).StockCount);
        }

        [Fact]
        public void Calculates_the_total_from_the_products_price()
        {
            var product = OrdersControllerTest.InsertProduct(Db, vendorId: "vendor", price: 100m);
            LoginAs("buyer");

            var order = OrdersController.Buy(new BuyRequest(product.Id, 3));

            Assert.Equal(100m, order.UnitPriceDkk);
            Assert.Equal(300m, order.TotalPriceDkk);
            Assert.False(order.DiscountApplied);
        }

        [Fact]
        public void Takes_the_buyer_from_the_login_and_the_vendor_from_the_product()
        {
            var product = OrdersControllerTest.InsertProduct(Db, vendorId: "vendor");
            LoginAs("buyer");

            var order = OrdersController.Buy(new BuyRequest(product.Id, 1));

            Assert.Equal("buyer", order.BuyerId);
            Assert.Equal("vendor", order.VendorId);
        }

        [Fact]
        public void Rejects_a_quantity_below_one()
        {
            var product = OrdersControllerTest.InsertProduct(Db, vendorId: "vendor");
            LoginAs("buyer");

            Assert.Throws<ValidationException>(() => OrdersController.Buy(new BuyRequest(product.Id, 0)));
        }

        [Fact]
        public void Rejects_an_unknown_product()
        {
            LoginAs("buyer");

            Assert.Throws<KeyNotFoundException>(() => OrdersController.Buy(new BuyRequest("nope", 1)));
        }

        [Fact]
        public void Rejects_buying_your_own_product()
        {
            var product = OrdersControllerTest.InsertProduct(Db, vendorId: "vendor");
            LoginAs("vendor");

            Assert.Throws<InvalidOperationException>(() => OrdersController.Buy(new BuyRequest(product.Id, 1)));
        }

        [Fact]
        public void Rejects_more_than_is_in_stock_and_changes_nothing()
        {
            var product = OrdersControllerTest.InsertProduct(Db, vendorId: "vendor", stock: 2);
            LoginAs("buyer");

            Assert.Throws<InvalidOperationException>(() => OrdersController.Buy(new BuyRequest(product.Id, 3)));

            // Nothing may have changed: no order, and the stock is the same
            Assert.Empty(Db.Orders().ToList());
            Assert.Equal(2, Db.Products().Single(p => p.Id == product.Id).StockCount);
        }

        [Fact]
        public void Rejects_a_sold_out_product()
        {
            var product = OrdersControllerTest.InsertProduct(Db, vendorId: "vendor", stock: 0);
            LoginAs("buyer");

            Assert.Throws<InvalidOperationException>(() => OrdersController.Buy(new BuyRequest(product.Id, 1)));
        }

        [Fact]
        public void Rejects_when_nobody_is_logged_in()
        {
            var product = OrdersControllerTest.InsertProduct(Db, vendorId: "vendor");

            // No LoginAs here, so there is no logged-in user
            Assert.Throws<UnauthorizedAccessException>(() => OrdersController.Buy(new BuyRequest(product.Id, 1)));
        }
    }

    #endregion

    #region Tests: GetMine

    public class GetMineTests : ApiTest
    {
        [Fact]
        public void Returns_only_the_logged_in_users_orders()
        {
            var product = OrdersControllerTest.InsertProduct(Db, vendorId: "vendor", stock: 10);

            LoginAs("alice");
            OrdersController.Buy(new BuyRequest(product.Id, 1));
            OrdersController.Buy(new BuyRequest(product.Id, 1));

            LoginAs("bob");
            OrdersController.Buy(new BuyRequest(product.Id, 1));

            var bobsOrders = OrdersController.GetMine();

            Assert.Single(bobsOrders);
            Assert.Equal("bob", bobsOrders[0].BuyerId);
        }
    }

    #endregion
}