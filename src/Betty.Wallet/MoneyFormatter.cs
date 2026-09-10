using System.Globalization;

namespace Betty.Wallet;

public static class MoneyFormatter
{
    public static string Format(decimal amount)
    {
        return $"${amount.ToString("0.00", CultureInfo.InvariantCulture)}";
    }
}
