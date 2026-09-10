namespace Betty.Wallet.Domain;

public static class MoneyRules
{
    public static decimal RoundToCents(decimal amount)
    {
        return decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
    }

    public static void EnsurePositiveCurrencyAmount(decimal amount, string parameterName)
    {
        if (amount <= 0)
        {
            throw new WalletRuleException("Amount must be greater than zero.");
        }

        EnsureCurrencyAmount(amount, parameterName);
    }

    public static void EnsureNonNegativeCurrencyAmount(decimal amount, string parameterName)
    {
        if (amount < 0)
        {
            throw new WalletRuleException("Amount cannot be negative.");
        }

        EnsureCurrencyAmount(amount, parameterName);
    }

    private static void EnsureCurrencyAmount(decimal amount, string parameterName)
    {
        if (amount != RoundToCents(amount))
        {
            throw new WalletRuleException("Amount cannot have more than two decimal places.");
        }
    }
}
