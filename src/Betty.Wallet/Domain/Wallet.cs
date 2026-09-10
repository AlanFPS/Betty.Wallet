namespace Betty.Wallet.Domain;

public sealed class Wallet
{
    public decimal Balance { get; private set; }

    public void Deposit(decimal amount)
    {
        MoneyRules.EnsurePositiveCurrencyAmount(amount, nameof(amount));

        try
        {
            Balance = MoneyRules.RoundToCents(Balance + amount);
        }
        catch (OverflowException exception)
        {
            throw new WalletRuleException("Amount is too large.", exception);
        }
    }

    public void Withdraw(decimal amount)
    {
        MoneyRules.EnsurePositiveCurrencyAmount(amount, nameof(amount));

        if (amount > Balance)
        {
            throw new WalletRuleException("Insufficient funds.");
        }

        Balance = MoneyRules.RoundToCents(Balance - amount);
    }

    public void SettleBet(decimal betAmount, decimal winAmount)
    {
        MoneyRules.EnsurePositiveCurrencyAmount(betAmount, nameof(betAmount));
        MoneyRules.EnsureNonNegativeCurrencyAmount(winAmount, nameof(winAmount));

        if (betAmount > Balance)
        {
            throw new WalletRuleException("Insufficient funds.");
        }

        try
        {
            Balance = MoneyRules.RoundToCents(Balance - betAmount + winAmount);
        }
        catch (OverflowException exception)
        {
            throw new WalletRuleException("Amount is too large.", exception);
        }
    }
}
