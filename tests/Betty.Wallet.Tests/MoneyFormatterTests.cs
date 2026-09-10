namespace Betty.Wallet.Tests;

public sealed class MoneyFormatterTests
{
    [Theory]
    [InlineData("0", "$0.00")]
    [InlineData("7.5", "$7.50")]
    [InlineData("10", "$10.00")]
    [InlineData("10.01", "$10.01")]
    public void FormatDisplaysTwoDecimalPlaces(string amountText, string expected)
    {
        var amount = decimal.Parse(amountText, System.Globalization.CultureInfo.InvariantCulture);

        Assert.Equal(expected, MoneyFormatter.Format(amount));
    }
}
