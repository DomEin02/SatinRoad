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
    private static User InsertVendor(SatinRoadDatabase db, string username = "TestVendor", bool isShutDown = false)
    {
        var vendor = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = username,
            PasswordHash = "seed-placeholder",
            Role = UserRole.User,
            IsShutDown = isShutDown,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Insert(vendor);
        return vendor;
    }

    // opretter et produkt direkte i DB'en, uden om controlleren
    private static Product InsertProduct(SatinRoadDatabase db, string vendorId, string categoryId,
        string name = "Sword", int stock = 5)
    {
        var product = new Product
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Description = "For testing",
            PriceDkk = 100m,
            StockCount = stock,
            CategoryId = categoryId,
            VendorId = vendorId,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Insert(product);
        return product;
    }

    #region Tests: GetAll

    public class GetAllTests : ApiTest
    {
        [Fact]
        public void Returns_every_product()
        {
            var category = ProductsControllerTest.InsertCategory(Db);
            var vendor = ProductsControllerTest.InsertVendor(Db);
            ProductsControllerTest.InsertProduct(Db, vendor.Id, category.Id, "Sword");
            ProductsControllerTest.InsertProduct(Db, vendor.Id, category.Id, "Shield");

            var result = ProductsController.GetAll();

            Assert.Equal(2, result.Count);
        }
    }

    #endregion

    #region Tests: Create

    public class CreateTests : ApiTest
    {
        [Fact]
        public void Inserts_the_product_for_the_logged_in_user()
        {
            var category = ProductsControllerTest.InsertCategory(Db);
            var vendor = ProductsControllerTest.InsertVendor(Db);
            LoginAs(vendor.Id);

            var created = ProductsController.Create(new ProductCreateRequest("Sword", "Sharp", 100m, 5, category.Id));

            Assert.Equal("Sword", created.Name);
            Assert.Equal(category.Id, created.CategoryId);
            Assert.Equal(vendor.Id, created.VendorId);
        }

        [Fact]
        public void Rejects_a_blank_name()
        {
            var category = ProductsControllerTest.InsertCategory(Db);
            var vendor = ProductsControllerTest.InsertVendor(Db);
            LoginAs(vendor.Id);

            Assert.Throws<ValidationException>(() =>
                ProductsController.Create(new ProductCreateRequest(" ", "desc", 100m, 5, category.Id)));
        }

        [Fact]
        public void Rejects_a_negative_price()
        {
            var category = ProductsControllerTest.InsertCategory(Db);
            var vendor = ProductsControllerTest.InsertVendor(Db);
            LoginAs(vendor.Id);

            Assert.Throws<ValidationException>(() =>
                ProductsController.Create(new ProductCreateRequest("Sword", "desc", -1m, 5, category.Id)));
        }

        [Fact]
        public void Rejects_a_negative_stock_count()
        {
            var category = ProductsControllerTest.InsertCategory(Db);
            var vendor = ProductsControllerTest.InsertVendor(Db);
            LoginAs(vendor.Id);

            Assert.Throws<ValidationException>(() =>
                ProductsController.Create(new ProductCreateRequest("Sword", "desc", 100m, -1, category.Id)));
        }

        [Fact]
        public void Rejects_an_unknown_category()
        {
            var vendor = ProductsControllerTest.InsertVendor(Db);
            LoginAs(vendor.Id);

            Assert.Throws<KeyNotFoundException>(() =>
                ProductsController.Create(new ProductCreateRequest("Sword", "desc", 100m, 5, Guid.NewGuid().ToString())));
        }

        [Fact]
        public void Rejects_when_not_logged_in()
        {
            var category = ProductsControllerTest.InsertCategory(Db);

            Assert.Throws<UnauthorizedAccessException>(() =>
                ProductsController.Create(new ProductCreateRequest("Sword", "Sharp", 100m, 5, category.Id)));
        }

        [Fact]
        public void Rejects_a_vendor_that_has_been_shut_down()
        {
            var category = ProductsControllerTest.InsertCategory(Db);
            var vendor = ProductsControllerTest.InsertVendor(Db, isShutDown: true);
            LoginAs(vendor.Id);

            Assert.Throws<UnauthorizedAccessException>(() =>
                ProductsController.Create(new ProductCreateRequest("Sword", "Sharp", 100m, 5, category.Id)));
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
            var product = ProductsControllerTest.InsertProduct(Db, vendor.Id, category.Id);
            LoginAs(vendor.Id);

            var updated = ProductsController.Update(new ProductUpdateRequest(product.Id, "Longsword", "Sharper", 120m, 4, category.Id));

            Assert.Equal("Longsword", updated.Name);
            Assert.Equal(120m, updated.PriceDkk);
        }

        [Fact]
        public void Throws_when_the_product_does_not_exist()
        {
            var category = ProductsControllerTest.InsertCategory(Db);
            var vendor = ProductsControllerTest.InsertVendor(Db);
            LoginAs(vendor.Id);

            Assert.Throws<KeyNotFoundException>(() =>
                ProductsController.Update(new ProductUpdateRequest(Guid.NewGuid().ToString(), "X", "Y", 1m, 1, category.Id)));
        }

        [Fact]
        public void Rejects_someone_elses_product()
        {
            var category = ProductsControllerTest.InsertCategory(Db);
            var owner = ProductsControllerTest.InsertVendor(Db, "Owner");
            var other = ProductsControllerTest.InsertVendor(Db, "Other");
            var product = ProductsControllerTest.InsertProduct(Db, owner.Id, category.Id);
            LoginAs(other.Id);

            Assert.Throws<UnauthorizedAccessException>(() =>
                ProductsController.Update(new ProductUpdateRequest(product.Id, "Hacked", "Hacked", 1m, 1, category.Id)));

            Assert.Equal("Sword", Db.Products().Single(p => p.Id == product.Id).Name);
        }

        [Fact]
        public void Rejects_when_not_logged_in()
        {
            var category = ProductsControllerTest.InsertCategory(Db);
            var vendor = ProductsControllerTest.InsertVendor(Db);
            var product = ProductsControllerTest.InsertProduct(Db, vendor.Id, category.Id);

            Assert.Throws<UnauthorizedAccessException>(() =>
                ProductsController.Update(new ProductUpdateRequest(product.Id, "X", "Y", 1m, 1, category.Id)));
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
            var product = ProductsControllerTest.InsertProduct(Db, vendor.Id, category.Id);
            LoginAs(vendor.Id);

            ProductsController.Delete(product.Id);

            Assert.Empty(ProductsController.GetAll());
        }

        [Fact]
        public void Throws_when_the_product_does_not_exist()
        {
            var vendor = ProductsControllerTest.InsertVendor(Db);
            LoginAs(vendor.Id);

            Assert.Throws<KeyNotFoundException>(() => ProductsController.Delete(Guid.NewGuid().ToString()));
        }

        [Fact]
        public void Rejects_someone_elses_product()
        {
            var category = ProductsControllerTest.InsertCategory(Db);
            var owner = ProductsControllerTest.InsertVendor(Db, "Owner");
            var other = ProductsControllerTest.InsertVendor(Db, "Other");
            var product = ProductsControllerTest.InsertProduct(Db, owner.Id, category.Id);
            LoginAs(other.Id);

            Assert.Throws<UnauthorizedAccessException>(() => ProductsController.Delete(product.Id));

            Assert.Single(ProductsController.GetAll());
        }

        [Fact]
        public void Rejects_when_not_logged_in()
        {
            var category = ProductsControllerTest.InsertCategory(Db);
            var vendor = ProductsControllerTest.InsertVendor(Db);
            var product = ProductsControllerTest.InsertProduct(Db, vendor.Id, category.Id);

            Assert.Throws<UnauthorizedAccessException>(() => ProductsController.Delete(product.Id));
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
            ProductsControllerTest.InsertProduct(Db, vendorA.Id, category.Id, "Sword");
            ProductsControllerTest.InsertProduct(Db, vendorA.Id, category.Id, "Shield");
            ProductsControllerTest.InsertProduct(Db, vendorB.Id, category.Id, "Potion");

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
            var product = ProductsControllerTest.InsertProduct(Db, vendor.Id, category.Id, stock: 5);
            LoginAs(vendor.Id);

            var updated = ProductsController.AdjustStock(new StockAdjustmentRequest(product.Id, 10));

            Assert.Equal(15, updated.StockCount);
        }

        [Fact]
        public void Decreases_the_stock_count()
        {
            var category = ProductsControllerTest.InsertCategory(Db);
            var vendor = ProductsControllerTest.InsertVendor(Db);
            var product = ProductsControllerTest.InsertProduct(Db, vendor.Id, category.Id, stock: 5);
            LoginAs(vendor.Id);

            var updated = ProductsController.AdjustStock(new StockAdjustmentRequest(product.Id, -3));

            Assert.Equal(2, updated.StockCount);
        }

        [Fact]
        public void Rejects_a_change_that_would_make_stock_negative()
        {
            var category = ProductsControllerTest.InsertCategory(Db);
            var vendor = ProductsControllerTest.InsertVendor(Db);
            var product = ProductsControllerTest.InsertProduct(Db, vendor.Id, category.Id, stock: 5);
            LoginAs(vendor.Id);

            Assert.Throws<ValidationException>(() =>
                ProductsController.AdjustStock(new StockAdjustmentRequest(product.Id, -10)));
        }

        [Fact]
        public void Throws_when_the_product_does_not_exist()
        {
            var vendor = ProductsControllerTest.InsertVendor(Db);
            LoginAs(vendor.Id);

            Assert.Throws<KeyNotFoundException>(() =>
                ProductsController.AdjustStock(new StockAdjustmentRequest(Guid.NewGuid().ToString(), 5)));
        }

        [Fact]
        public void Rejects_someone_elses_product()
        {
            var category = ProductsControllerTest.InsertCategory(Db);
            var owner = ProductsControllerTest.InsertVendor(Db, "Owner");
            var other = ProductsControllerTest.InsertVendor(Db, "Other");
            var product = ProductsControllerTest.InsertProduct(Db, owner.Id, category.Id, stock: 5);
            LoginAs(other.Id);

            Assert.Throws<UnauthorizedAccessException>(() =>
                ProductsController.AdjustStock(new StockAdjustmentRequest(product.Id, 10)));

            Assert.Equal(5, Db.Products().Single(p => p.Id == product.Id).StockCount);
        }

        [Fact]
        public void Rejects_when_not_logged_in()
        {
            var category = ProductsControllerTest.InsertCategory(Db);
            var vendor = ProductsControllerTest.InsertVendor(Db);
            var product = ProductsControllerTest.InsertProduct(Db, vendor.Id, category.Id);

            Assert.Throws<UnauthorizedAccessException>(() =>
                ProductsController.AdjustStock(new StockAdjustmentRequest(product.Id, 1)));
        }
    }

    #endregion
}