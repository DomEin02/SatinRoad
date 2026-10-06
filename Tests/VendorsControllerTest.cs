using Infa;
using LinqToDB;
using LinqToDB.Data;
using Xunit;

namespace Tests;

public class VendorsControllerTest
{
    private static User InsertUser(SatinRoadDatabase db, string username, bool isShutDown = false)
    {
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = username,
            PasswordHash = "x",
            Role = UserRole.User,
            IsShutDown = isShutDown,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Insert(user);
        return user;
    }

    private static void InsertOrders(SatinRoadDatabase db, string vendorId, int count)
    {
        var orders = Enumerable.Range(0, count).Select(_ => new Order
        {
            Id = Guid.NewGuid().ToString(),
            BuyerId = "buyer",
            VendorId = vendorId,
            ProductId = "p",
            Quantity = 1,
            UnitPriceDkk = 10m,
            DiscountApplied = false,
            TotalPriceDkk = 10m,
            CreatedAtUtc = DateTime.UtcNow
        }).ToList();
        db.BulkCopy(orders);
    }
    
    #region Tests: GetFeatured

    public class GetFeaturedTests : ApiTest
    {
        [Fact]
        public void A_vendor_with_more_than_100_orders_is_featured()
        {
            var vendor = VendorsControllerTest.InsertUser(Db, "BigVendor");
            VendorsControllerTest.InsertOrders(Db, vendor.Id, 101);

            var result = VendorsController.GetFeatured();
            
            Assert.Single(result);
            Assert.Equal(vendor.Id, result[0].VendorId);
            Assert.Equal(101, result[0].OrderCount);
        }

        [Fact]
        public void Exactly_100_orders_is_not_enough()
        {
            var vendor = VendorsControllerTest.InsertUser(Db, "AlmostVendor");
            VendorsControllerTest.InsertOrders(Db, vendor.Id, 100);
            
            Assert.Empty(VendorsController.GetFeatured());
        }

        [Fact]
        public void A_shut_down_vendor_is_never_featured()
        {
            var vendor = VendorsControllerTest.InsertUser(Db, "ShutVendor", isShutDown: true);
            VendorsControllerTest.InsertOrders(Db, vendor.Id, 150);
            
            Assert.Empty(VendorsController.GetFeatured());
        }

        [Fact]
        public void The_vendor_with_most_orders_comes_first()
        {
            var smaller = VendorsControllerTest.InsertUser(Db, "Smaller");
            var bigger = VendorsControllerTest.InsertUser(Db, "Bigger");
            VendorsControllerTest.InsertOrders(Db, smaller.Id, 120);
            VendorsControllerTest.InsertOrders(Db, bigger.Id, 150);

            var result = VendorsController.GetFeatured();
            
            Assert.Equal(2, result.Count);
            Assert.Equal(bigger.Id, result[0].VendorId);
            Assert.Equal(smaller.Id, result[1].VendorId);
        }

        [Fact]
        public void Returns_nothing_when_no_vendor_qualifies()
        {
            Assert.Empty(VendorsController.GetFeatured());
        }
    }
    
    #endregion
}