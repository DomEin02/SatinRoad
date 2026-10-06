using System.Security.Claims;
using API.Controllers;
using Infa;
using LinqToDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace Tests;

public abstract class ApiTest : IDisposable
{
    private readonly string _dbPath;
    protected readonly SatinRoadDatabase Db;
    protected readonly CategoriesController CategoriesController;
    protected readonly ProductsController ProductsController;
    protected readonly AuthController AuthController;
    protected readonly OrdersController OrdersController;
    protected readonly VendorsController  VendorsController;

    protected ApiTest()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"satinroad-test-{Guid.NewGuid()}.db");
        var options = new DataOptions().UseSQLite($"Data Source={_dbPath}");
        Db = new SatinRoadDatabase(new DataOptions<SatinRoadDatabase>(options));

        Db.CreateTable<User>(tableOptions: TableOptions.CreateIfNotExists);
        Db.CreateTable<Category>(tableOptions: TableOptions.CreateIfNotExists);
        Db.CreateTable<Product>(tableOptions: TableOptions.CreateIfNotExists);
        Db.CreateTable<Order>(tableOptions: TableOptions.CreateIfNotExists);

        CategoriesController = new CategoriesController(Db);
        ProductsController = new ProductsController(Db);
        OrdersController = new OrdersController(Db);
        VendorsController = new VendorsController(Db);

        // AuthController Jwt Settings
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-secret-key-that-is-at-least-32-characters",
                ["Jwt:Issuer"] = "SatinRoad"
            })
            .Build();
        AuthController = new AuthController(Db, config);
    }

    // Pretends this user is logged in, by giving the controller the same claim a real token carries
    protected void LoginAs(string userId)
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "Test");
        OrdersController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    public void Dispose()
    {
        Db.Dispose();

        SqliteConnection.ClearAllPools();

        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }
}