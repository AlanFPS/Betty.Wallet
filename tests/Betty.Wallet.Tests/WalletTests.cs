using Betty.Wallet.Domain;
using PlayerWallet = Betty.Wallet.Domain.Wallet;

namespace Betty.Wallet.Tests;

public sealed class WalletTests
{
    [Fact]
    public void StartsWithZeroBalance()
    {
        var wallet = new PlayerWallet();

        Assert.Equal(0m, wallet.Balance);
    }

    [Fact]
    public void DepositAddsFunds()
    {
        var wallet = new PlayerWallet();

        wallet.Deposit(12.34m);

        Assert.Equal(12.34m, wallet.Balance);
    }

    [Fact]
    public void WithdrawSubtractsFunds()
    {
        var wallet = new PlayerWallet();

        wallet.Deposit(20m);
        wallet.Withdraw(7.25m);

        Assert.Equal(12.75m, wallet.Balance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DepositRejectsNonPositiveAmounts(decimal amount)
    {
        var wallet = new PlayerWallet();

        var exception = Assert.Throws<WalletRuleException>(() => wallet.Deposit(amount));

        Assert.Equal("Amount must be greater than zero.", exception.Message);
        Assert.Equal(0m, wallet.Balance);
    }

    [Fact]
    public void DepositRejectsMoreThanTwoDecimals()
    {
        var wallet = new PlayerWallet();

        var exception = Assert.Throws<WalletRuleException>(() => wallet.Deposit(10.001m));

        Assert.Equal("Amount cannot have more than two decimal places.", exception.Message);
        Assert.Equal(0m, wallet.Balance);
    }

    [Fact]
    public void WithdrawRejectsOverdraftAndLeavesBalanceUnchanged()
    {
        var wallet = new PlayerWallet();
        wallet.Deposit(10m);

        var exception = Assert.Throws<WalletRuleException>(() => wallet.Withdraw(10.01m));

        Assert.Equal("Insufficient funds.", exception.Message);
        Assert.Equal(10m, wallet.Balance);
    }

    [Fact]
    public void SettleBetUsesBetMinusWinFormula()
    {
        var wallet = new PlayerWallet();
        wallet.Deposit(50m);

        wallet.SettleBet(10m, 15.35m);

        Assert.Equal(55.35m, wallet.Balance);
    }

    [Fact]
    public void SettleBetRejectsInsufficientFundsAndLeavesBalanceUnchanged()
    {
        var wallet = new PlayerWallet();
        wallet.Deposit(5m);

        var exception = Assert.Throws<WalletRuleException>(() => wallet.SettleBet(10m, 0m));

        Assert.Equal("Insufficient funds.", exception.Message);
        Assert.Equal(5m, wallet.Balance);
    }

    [Fact]
    public void DepositRejectsOverflowAndLeavesBalanceUnchanged()
    {
        var wallet = new PlayerWallet();
        wallet.Deposit(decimal.MaxValue);

        var exception = Assert.Throws<WalletRuleException>(() => wallet.Deposit(1m));

        Assert.Equal("Amount is too large.", exception.Message);
        Assert.Equal(decimal.MaxValue, wallet.Balance);
    }

    [Fact]
    public void SettleBetRejectsOverflowAndLeavesBalanceUnchanged()
    {
        var wallet = new PlayerWallet();
        wallet.Deposit(decimal.MaxValue);

        var exception = Assert.Throws<WalletRuleException>(() => wallet.SettleBet(1m, 10m));

        Assert.Equal("Amount is too large.", exception.Message);
        Assert.Equal(decimal.MaxValue, wallet.Balance);
    }
}
