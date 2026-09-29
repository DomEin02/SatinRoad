using LinqToDB.Mapping;

namespace Infa;

[Table("Categories")]
public class Category
{
    [PrimaryKey] public string Id { get; set; } = "";

    [Column] [NotNull] public string Name { get; set; } = "";
}