using System.ComponentModel.DataAnnotations;
using API.Dtos;
using Infa;
using LinqToDB;
using Xunit;

namespace Tests;

public class ProductsControllerTest
{
    // opretter en kategori direkte i DB'en, uden om controlleren
    private static Category InsertCategory(SatinRoadDatabase db, string name = "Test Category")
    {
        var category = new Category { Id = Guid.NewGuid().ToString(), Name = name };
        db.Insert(category);
        return category;
    }

    // opretter en vendor (bruger) direkte i DB'en
    private static User InsertVendor(SatinRoadDatabase db, string username = "TestVendor")
    {
        var vendor = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = username,
            PasswordHash = "seed-placeholder",
            Role = UserRole.User,
            IsShutDown = false,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Insert(vendor);
        return vendor;
    }

    #region Tests: GetAll

    public class GetAllTests : ApiTest
    {
        [Fact]
        public void Returns_every_product()
        {
            var category = ProductsControllerTest.InsertCategory(Db);
            var vendor = ProductsControllerTest.InsertVendor(Db);
            ProductsController.Create(new ProductCreateRequest("Sword", "Sharp", 100m, 5, category.Id, vendor.Id));
            ProductsController.Create(new ProductCreateRequest("Shield", "Sturdy", 50m, 3, category.Id, vendor.Id));

            var result = ProductsController.GetAll();
            Assert.Equal(2, result.Count);
        }
    }

    #endregion

    #region Tests: Create

    public class CreateTests : ApiTest
    {
        [Fact]
        public void Inserts_the_product_and_returns_it()
        {
            var category = ProductsControllerTest.InsertCategory(Db);
            var vendor = ProductsControllerTest.InsertVendor(Db);

            var created = ProductsController.Create(new ProductCreateRequest("Sword", "Sharp", 100m, 5, category.Id, vendor.Id));

            Assert.Equal("Sword", created.Name);
            Assert.Equal(category.Id, created.CategoryId);
            Assert.Equal(vendor.Id, created.VendorId);
        }

        [Fact]
        public void Rejects_a_blank_name()
        {
            var category = ProductsControllerTest.InsertCategory(Db);
            var vendor = ProductsControllerTest.InsertVendor(Db);

            Assert.Throws<ValidationException>(() =>
                ProductsController.Create(new ProductCreateRequest(" ", "desc", 100m, 5, category.Id, vendor.Id)));
        }

        [Fact]
        public void Rejects_a_negative_price()
        {
            var category = ProductsControllerTest.InsertCategory(Db);
            var vendor = ProductsControllerTest.InsertVendor(Db);

            Assert.Throws<ValidationException>(() =>
                ProductsController.Create(new ProductCreateRequest("Sword", "desc", -1m, 5, category.Id, vendor.Id)));
        }

        [Fact]
        public void Rejects_a_negative_stock_count()
        {
            var category = ProductsControllerTest.InsertCategory(Db);
            var vendor = ProductsControllerTest.InsertVendor(Db);

            Assert.Throws<ValidationException>(() =>
                ProductsController.Create(new ProductCreateRequest("Sword", "desc", 100m, -1, category.Id, vendor.Id)));
        }

        [Fact]
        public void Rejects_an_unknown_category()
        {
            var vendor = ProductsControllerTest.InsertVendor(Db);

            Assert.Throws<KeyNotFoundException>(() =>
                ProductsController.Create(new ProductCreateRequest("Sword", "desc", 100m, 5, Guid.NewGuid().ToString(), vendor.Id)));
        }
    }

    #endregion

    #region Tests: Update

    public class UpdateTests : ApiTest
    {
        [Fact]
        public void Changes_the_product()
        {
            var category = ProductsControllerTest.InsertCategory(Db);
            var vendor = ProductsControllerTest.InsertVendor(Db);
            var created = ProductsController.Create(new ProductCreateRequest("Sword", "Sharp", 100m, 5, category.Id, vendor.Id));

            var updated = ProductsController.Update(new ProductUpdateRequest(created.Id, "Longsword", "Sharper", 120m, 4, category.Id));

            Assert.Equal("Longsword", updated.Name);
            Assert.Equal(120m, updated.PriceDkk);
        }

        [Fact]
        public void Throws_when_the_product_does_not_exist()
        {
            var category = ProductsControllerTest.InsertCategory(Db);

            Assert.Throws<KeyNotFoundException>(() =>
                ProductsController.Update(new ProductUpdateRequest(Guid.NewGuid().ToString(), "X", "Y", 1m, 1, category.Id)));
        }
    }

    #endregion

    #region Tests: Delete

    public class DeleteTests : ApiTest
    {
        [Fact]
        public void Removes_the_product()
        {
            var category = ProductsControllerTest.InsertCategory(Db);
            var vendor = ProductsControllerTest.InsertVendor(Db);
            var created = ProductsController.Create(new ProductCreateRequest("Sword", "Sharp", 100m, 5, category.Id, vendor.Id));

            ProductsController.Delete(created.Id);

            Assert.Empty(ProductsController.GetAll());
        }

        [Fact]
        public void Throws_when_the_product_does_not_exist()
        {
            Assert.Throws<KeyNotFoundException>(() => ProductsController.Delete(Guid.NewGuid().ToString()));
        }
    }

    #endregion
    
    #region Tests: GetByVendor

    public class GetByVendorTests : ApiTest
    {
        [Fact]
        public void Returns_only_that_vendors_products()
        {
            var category = ProductsControllerTest.InsertCategory(Db);
            var vendorA = ProductsControllerTest.InsertVendor(Db, "VendorA");
            var vendorB = ProductsControllerTest.InsertVendor(Db, "VendorB");
            
            ProductsController.Create(new ProductCreateRequest("Sword", "Sharp", 100m, 5, category.Id, vendorA.Id));
            ProductsController.Create(new ProductCreateRequest("Shield", "Sturdy", 50m, 3, category.Id, vendorA.Id));
            ProductsController.Create(new ProductCreateRequest("Potion", "Heals", 20m, 10, category.Id, vendorB.Id));

            var result = ProductsController.GetByVendor(vendorA.Id);
            
            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Equal(vendorA.Id, p.VendorId));
        }
        
        [Fact]
        public void Returns_empty_list_for_a_vendor_with_no_products()
        {
            var vendor = ProductsControllerTest.InsertVendor(Db);

            var result = ProductsController.GetByVendor(vendor.Id);

            Assert.Empty(result);
        }
    }
    
    #endregion

    #region Tests: AdjustStock

    public class AdjustStockTests : ApiTest
    {
        [Fact]
        public void Increases_the_stock_count()
        {
            var category = ProductsControllerTest.InsertCategory(Db);
            var vendor = ProductsControllerTest.InsertVendor(Db);
            var created = ProductsController.Create(new ProductCreateRequest("Sword", "Sharp", 100m, 5, category.Id, vendor.Id));

            var updated = ProductsController.AdjustStock(new StockAdjustmentRequest(created.Id, 10));

            Assert.Equal(15, updated.StockCount);
        }

        [Fact]
        public void Decreases_the_stock_count()
        {
            var category = ProductsControllerTest.InsertCategory(Db);
            var vendor = ProductsControllerTest.InsertVendor(Db);
            var created = ProductsController.Create(new ProductCreateRequest("Sword", "Sharp", 100m, 5, category.Id, vendor.Id));

            var updated = ProductsController.AdjustStock(new StockAdjustmentRequest(created.Id, -3));

            Assert.Equal(2, updated.StockCount);
        }

        [Fact]
        public void Rejects_a_change_that_would_make_stock_negative()
        {
            var category = ProductsControllerTest.InsertCategory(Db);
            var vendor = ProductsControllerTest.InsertVendor(Db);
            var created = ProductsController.Create(new ProductCreateRequest("Sword", "Sharp", 100m, 5, category.Id, vendor.Id));

            Assert.Throws<ValidationException>(() =>
                ProductsController.AdjustStock(new StockAdjustmentRequest(created.Id, -10)));
        }

        [Fact]
        public void Throws_when_the_product_does_not_exist()
        {
            Assert.Throws<KeyNotFoundException>(() =>
                ProductsController.AdjustStock(new StockAdjustmentRequest(Guid.NewGuid().ToString(), 5)));
        }
    }

    #endregion
}