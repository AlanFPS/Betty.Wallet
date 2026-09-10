using System.Globalization;
using Betty.Wallet.Domain;
using Betty.Wallet.Game;
using PlayerWallet = Betty.Wallet.Domain.Wallet;

namespace Betty.Wallet.Tests;

public sealed class ConsoleAppTests
{
    [Fact]
    public void RunProcessesScriptedSessionUntilExit()
    {
        var input = string.Join(
            Environment.NewLine,
            "deposit 10",
            "bet 5",
            "withdraw 6",
            "dance",
            "exit");
        var output = new StringWriter(CultureInfo.InvariantCulture);
        var app = new ConsoleApp(
            new StringReader(input),
            output,
            new PlayerWallet(),
            new SlotGame(new QueueRandomNumberGenerator(50, 150)));

        var exitCode = app.Run();

        Assert.Equal(0, exitCode);
        Assert.Equal(
            string.Join(
                Environment.NewLine,
                "Please, submit action:",
                "Your deposit of $10.00 was successful. Your current balance is: $10.00",
                "Please, submit action:",
                "Congrats - you won $7.50! Your current balance is: $12.50",
                "Please, submit action:",
                "Your withdrawal of $6.00 was successful. Your current balance is: $6.50",
                "Please, submit action:",
                "Unknown command 'dance'. Use deposit <amount>, withdraw <amount>, bet <amount>, or exit. Your current balance is: $6.50",
                "Please, submit action:",
                "Thank you for playing! Hope to see you again soon.",
                string.Empty),
            output.ToString());
    }

    [Fact]
    public void RunLeavesBalanceUnchangedAfterRejectedWithdrawal()
    {
        var input = string.Join(
            Environment.NewLine,
            "deposit 5",
            "withdraw 6",
            "exit");
        var output = new StringWriter(CultureInfo.InvariantCulture);
        var app = new ConsoleApp(
            new StringReader(input),
            output,
            new PlayerWallet(),
            new SlotGame(new QueueRandomNumberGenerator()));

        app.Run();

        Assert.Contains(
            "Insufficient funds. Your current balance is: $5.00",
            output.ToString(),
            StringComparison.Ordinal);
    }

    [Fact]
    public void RunRejectsOverflowAndContinues()
    {
        var input = string.Join(
            Environment.NewLine,
            $"deposit {decimal.MaxValue}",
            "deposit 1",
            "exit");
        var output = new StringWriter(CultureInfo.InvariantCulture);
        var app = new ConsoleApp(
            new StringReader(input),
            output,
            new PlayerWallet(),
            new SlotGame(new QueueRandomNumberGenerator()));

        app.Run();

        Assert.Contains(
            $"Amount is too large. Your current balance is: {MoneyFormatter.Format(decimal.MaxValue)}",
            output.ToString(),
            StringComparison.Ordinal);
        Assert.Contains("Thank you for playing! Hope to see you again soon.", output.ToString(), StringComparison.Ordinal);
    }
}
