namespace Betty.Wallet.Tests;

public sealed class CommandParserTests
{
    [Fact]
    public void TryParseAcceptsTabBetweenCommandAndAmount()
    {
        var parser = new CommandParser();

        var parsed = parser.TryParse("deposit\t10", out var command, out var error);

        Assert.True(parsed);
        Assert.Equal(string.Empty, error);
        Assert.Equal(CommandType.Deposit, command.Type);
        Assert.Equal(10m, command.Amount);
    }

    [Fact]
    public void TryParseAcceptsRepeatedSpaces()
    {
        var parser = new CommandParser();

        var parsed = parser.TryParse("withdraw   5", out var command, out var error);

        Assert.True(parsed);
        Assert.Equal(string.Empty, error);
        Assert.Equal(CommandType.Withdraw, command.Type);
        Assert.Equal(5m, command.Amount);
    }

    [Fact]
    public void TryParseAcceptsTabBeforeBetAmount()
    {
        var parser = new CommandParser();

        var parsed = parser.TryParse("bet\t1", out var command, out var error);

        Assert.True(parsed);
        Assert.Equal(string.Empty, error);
        Assert.Equal(CommandType.Bet, command.Type);
        Assert.Equal(1m, command.Amount);
    }

    [Fact]
    public void TryParseRejectsExitWithExtraArgument()
    {
        var parser = new CommandParser();

        var parsed = parser.TryParse("exit\t1", out _, out var error);

        Assert.False(parsed);
        Assert.Equal("Exit does not accept an amount.", error);
    }
}
