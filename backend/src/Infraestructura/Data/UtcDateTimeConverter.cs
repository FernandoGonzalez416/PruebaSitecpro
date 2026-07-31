using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MesaSitec.Infraestructura.Data;

public class UtcDateTimeConverter : ValueConverter<DateTime, string>
{
    public static readonly UtcDateTimeConverter Instance = new();

    public UtcDateTimeConverter()
        : base(
            v => v.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
            v => DateTime.Parse(v, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            new ConverterMappingHints(size: 35))
    {
    }
}

public class UtcNullableDateTimeConverter : ValueConverter<DateTime?, string?>
{
    public static readonly UtcNullableDateTimeConverter Instance = new();

    public UtcNullableDateTimeConverter()
        : base(
            v => v == null ? null : v.Value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
            v => v == null ? null : DateTime.Parse(v, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            new ConverterMappingHints(size: 35))
    {
    }
}
