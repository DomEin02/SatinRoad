using API.Controllers;
using Infa;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.Data.Sqlite;

namespace Tests;

public abstract class ApiTest : IDisposable
{
    private readonly string _dbPath;
    protected readonly SatinRoadDatabase Db;
    protected readonly CategoriesController CategoriesController;

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
    }

    public void Dispose()
    {
        Db.Dispose();
        
        SqliteConnection.ClearAllPools();

        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }
}