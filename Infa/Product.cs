using LinqToDB.Mapping;

namespace Infa;

[Table("Products")]
public class Product
{
    [PrimaryKey] public string Id { get; set; } = "";

    [Column] [NotNull] public string Name { get; set; } = "";
    [Column] [NotNull] public string Description { get; set; } = "";
    [Column] public decimal PriceDkk { get; set; }
    [Column] public int StockCount { get; set; }

    [Column] [NotNull] public string CategoryId { get; set; } = "";
    [Column] [NotNull] public string VendorId { get; set; } = "";

    [Column] [ValueConverter(ConverterType = typeof(UtcDateTimeConverter))] public DateTime CreatedAtUtc { get; set; }
}