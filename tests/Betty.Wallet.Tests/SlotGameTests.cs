using System.Globalization;
using Betty.Wallet.Domain;
using Betty.Wallet.Game;

namespace Betty.Wallet.Tests;

public sealed class SlotGameTests
{
    [Theory]
    [InlineData("0.99")]
    [InlineData("10.01")]
    public void PlayRejectsBetsOutsideAllowedRange(string betAmountText)
    {
        var betAmount = decimal.Parse(betAmountText, CultureInfo.InvariantCulture);
        var game = new SlotGame(new QueueRandomNumberGenerator());

        var exception = Assert.Throws<WalletRuleException>(() => game.Play(betAmount));

        Assert.Equal("Bet amount must be between $1.00 and $10.00.", exception.Message);
    }

    [Fact]
    public void PlayRejectsBetsWithMoreThanTwoDecimals()
    {
        var game = new SlotGame(new QueueRandomNumberGenerator());

        var exception = Assert.Throws<WalletRuleException>(() => game.Play(1.001m));

        Assert.Equal("Amount cannot have more than two decimal places.", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(49)]
    public void RollsBelowFiftyLose(int roll)
    {
        var game = new SlotGame(new QueueRandomNumberGenerator(roll));

        var result = game.Play(10m);

        Assert.Equal(GameOutcome.Loss, result.Outcome);
        Assert.Equal(0m, result.WinAmount);
        Assert.Equal(0m, result.Multiplier);
    }

    [Fact]
    public void RollFiftyStartsRegularWinBucket()
    {
        var game = new SlotGame(new QueueRandomNumberGenerator(50, 100));

        var result = game.Play(10m);

        Assert.Equal(GameOutcome.RegularWin, result.Outcome);
        Assert.Equal(1m, result.Multiplier);
        Assert.Equal(10m, result.WinAmount);
    }

    [Fact]
    public void RollEightyNineEndsRegularWinBucket()
    {
        var game = new SlotGame(new QueueRandomNumberGenerator(89, 200));

        var result = game.Play(10m);

        Assert.Equal(GameOutcome.RegularWin, result.Outcome);
        Assert.Equal(2m, result.Multiplier);
        Assert.Equal(20m, result.WinAmount);
    }

    [Fact]
    public void RollNinetyStartsBigWinBucket()
    {
        var game = new SlotGame(new QueueRandomNumberGenerator(90, 201));

        var result = game.Play(10m);

        Assert.Equal(GameOutcome.BigWin, result.Outcome);
        Assert.Equal(2.01m, result.Multiplier);
        Assert.Equal(20.10m, result.WinAmount);
    }

    [Fact]
    public void RollNinetyNineAllowsTenTimesWin()
    {
        var game = new SlotGame(new QueueRandomNumberGenerator(99, 1000));

        var result = game.Play(10m);

        Assert.Equal(GameOutcome.BigWin, result.Outcome);
        Assert.Equal(10m, result.Multiplier);
        Assert.Equal(100m, result.WinAmount);
    }

    [Fact]
    public void WinAmountRoundsToCents()
    {
        var game = new SlotGame(new QueueRandomNumberGenerator(50, 133));

        var result = game.Play(7.77m);

        Assert.Equal(1.33m, result.Multiplier);
        Assert.Equal(10.33m, result.WinAmount);
    }
}
