using ClosedXML.Excel;

namespace AdeelBrotherCement.Infrastructure.Excel;

internal static class ExcelCellExtensions
{
    public static decimal GetDecimal(this IXLCell cell)
    {
        if (cell.TryGetValue(out decimal dec)) return dec;
        if (cell.TryGetValue(out double dbl)) return (decimal)dbl;
        if (decimal.TryParse(cell.GetString(), out var parsed)) return parsed;
        return 0;
    }

    public static bool GetBoolean(this IXLCell cell)
    {
        if (cell.TryGetValue(out bool b)) return b;
        return string.Equals(cell.GetString(), "true", StringComparison.OrdinalIgnoreCase);
    }
}
