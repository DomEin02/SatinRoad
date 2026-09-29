using LinqToDB.Mapping;

namespace Infa;

public enum UserRole
{
    [MapValue("user")] User,
    [MapValue("admin")] Admin
}

[Table("Users")]
public class User
{
    [PrimaryKey] public string Id { get; set; } = "";

    [Column] [NotNull] public string Username { get; set; } = "";
    [Column] [NotNull] public string PasswordHash { get; set; } = "";
    [Column] [NotNull] public UserRole Role { get; set; }
    [Column] public bool IsShutDown { get; set; }
    [Column] [ValueConverter(ConverterType = typeof(UtcDateTimeConverter))] public DateTime CreatedAtUtc { get; set; }
}