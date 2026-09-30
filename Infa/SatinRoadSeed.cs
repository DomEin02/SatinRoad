using LinqToDB;
using LinqToDB.Data;

namespace Infa;

public static class SatinRoadSeed
{
    public const string SeedPassword = "password123";

    public static void EnsureSeeded(SatinRoadDatabase db)
    {
        db.CreateTable<User>(tableOptions: TableOptions.CreateIfNotExists);
        db.CreateTable<Category>(tableOptions: TableOptions.CreateIfNotExists);
        db.CreateTable<Product>(tableOptions: TableOptions.CreateIfNotExists);
        db.CreateTable<Order>(tableOptions: TableOptions.CreateIfNotExists);
        if (db.Users().Any()) return;
        db.BulkCopy(BuildUsers());
        db.BulkCopy(BuildCategories());
        db.BulkCopy(BuildProducts());
        db.BulkCopy(BuildOrders());
    }

    public static string IdOf(int index)
    {
        return index.ToString();
    }

    public static User[] BuildUsers()
    {
        var id = 0;

        User Person(string username, UserRole role)
        {
            return new User
            {
                Id = IdOf(++id),
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(SeedPassword),
                Role = role,
                CreatedAtUtc = DateTime.UtcNow
            };
        }

        return
        [
            Person("admin", UserRole.Admin), // 1
            Person("ShadowMerchant", UserRole.User), // 2: vendor, 101 sales -> featured
            Person("RustyBlade", UserRole.User), // 3: vendor, 0 sales -> not featured
            Person("LoyalLarry", UserRole.User), // 4: buyer, 10 orders from ShadowMerchant -> discount test
            Person("NewbieNick", UserRole.User) // 5: buyer
        ];
    }

    public static Category[] BuildCategories()
    {
        var id = 0;

        Category Group(string name)
        {
            return new Category { Id = IdOf(++id), Name = name };
        }

        return
        [
            Group("Drugs"), // 1
            Group("Weaponry"), // 2
            Group("Stolen Artifacts") // 3
        ];
    }

    public static Product[] BuildProducts()
    {
        var id = 0;

        Product Item(string name, string description, decimal price, int stock, int category, int vendor)
        {
            return new Product
            {
                Id = IdOf(++id),
                Name = name,
                Description = description,
                PriceDkk = price,
                StockCount = stock,
                CategoryId = IdOf(category),
                VendorId = IdOf(vendor),
                CreatedAtUtc = DateTime.UtcNow
            };
        }

        return
        [
            Item("Suspiciously Strong Coffee Beans", "One cup and you'll hear colours.", 24.50m, 50, 1, 2), // 1
            Item("Extra-Sour Gummy Bears", "Banned in three countries for being too sour.", 9.99m, 200, 1, 2), // 2
            Item("Rusty Pirate Cutlass", "Previous owner no longer needs it.", 149.00m, 12, 2, 3), // 3
            Item("Slightly Cursed Pharaoh Mask", "Authentic. Mild curse. No refunds.", 4999.99m, 0, 3, 3) // 4: sold out
        ];
    }

    public static Order[] BuildOrders()
    {
        var id = 0;
        var orders = new List<Order>();

        void Buy(int buyer, int vendor, int product, decimal price, int times)
        {
            for (var i = 0; i < times; i++)
                orders.Add(new Order
                {
                    Id = IdOf(++id),
                    BuyerId = IdOf(buyer),
                    VendorId = IdOf(vendor),
                    ProductId = IdOf(product),
                    Quantity = 1,
                    UnitPriceDkk = price,
                    DiscountApplied = false,
                    TotalPriceDkk = price,
                    CreatedAtUtc = DateTime.UtcNow
                });
        }

        Buy(4, 2, 1, 24.50m, 10); // LoyalLarry buys coffee from ShadowMerchant 10 times
        Buy(5, 2, 2, 9.99m, 91); // NewbieNick buys gummy bears from ShadowMerchant 91 times

        return orders.ToArray();
    }
}