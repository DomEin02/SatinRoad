using LinqToDB.Mapping;

namespace Infa;

[Table("Orders")]
public class Order
{
    [PrimaryKey] public string Id { get; set; } = "";

    [Column] [NotNull] public string BuyerId { get; set; } = "";
    [Column] [NotNull] public string VendorId { get; set; } = "";
    [Column] [NotNull] public string ProductId { get; set; } = "";

    [Column] public int Quantity { get; set; }
    [Column] public decimal UnitPriceDkk { get; set; }
    [Column] public bool DiscountApplied { get; set; }
    [Column] public decimal TotalPriceDkk { get; set; }

    [Column] [ValueConverter(ConverterType = typeof(UtcDateTimeConverter))] public DateTime CreatedAtUtc { get; set; }
}