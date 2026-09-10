namespace Betty.Wallet.Domain;

public sealed class WalletRuleException : Exception
{
    public WalletRuleException(string message)
        : base(message)
    {
    }

    public WalletRuleException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
