using LinqToDB;
using LinqToDB.Data;

namespace Infa;

public class SatinRoadDatabase(DataOptions<SatinRoadDatabase> dataopts) : DataConnection(dataopts.Options)
{
    public ITable<User> Users()
    {
        return this.GetTable<User>();
    }

    public ITable<Category> Categories()
    {
        return this.GetTable<Category>();
    }

    public ITable<Product> Products()
    {
        return this.GetTable<Product>();
    }

    public ITable<Order> Orders()
    {
        return this.GetTable<Order>();
    }
}