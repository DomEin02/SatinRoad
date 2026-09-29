using LinqToDB.Mapping;

namespace Infa;

public class UtcDateTimeConverter() : ValueConverterFunc<DateTime, DateTime>(
    model => model,
    provider => DateTime.SpecifyKind(provider, DateTimeKind.Utc),
    false);