using Betty.Wallet.Domain;
using Betty.Wallet.Game;

namespace Betty.Wallet;

public sealed class ConsoleApp
{
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly CommandParser _parser;
    private readonly Domain.Wallet _wallet;
    private readonly SlotGame _game;

    public ConsoleApp(TextReader input, TextWriter output, Domain.Wallet wallet, SlotGame game)
    {
        _input = input;
        _output = output;
        _wallet = wallet;
        _game = game;
        _parser = new CommandParser();
    }

    public int Run()
    {
        while (true)
        {
            _output.WriteLine("Please, submit action:");

            var input = _input.ReadLine();
            if (input is null)
            {
                return 0;
            }

            if (!_parser.TryParse(input, out var command, out var error))
            {
                WriteRejected(error);
                continue;
            }

            if (command.Type == CommandType.Exit)
            {
                _output.WriteLine("Thank you for playing! Hope to see you again soon.");
                return 0;
            }

            Handle(command);
        }
    }

    private void Handle(ParsedCommand command)
    {
        try
        {
            var amount = command.Amount ?? throw new InvalidOperationException("Amount command missing amount.");

            switch (command.Type)
            {
                case CommandType.Deposit:
                    _wallet.Deposit(amount);
                    _output.WriteLine(
                        $"Your deposit of {MoneyFormatter.Format(amount)} was successful. Your current balance is: {MoneyFormatter.Format(_wallet.Balance)}");
                    break;

                case CommandType.Withdraw:
                    _wallet.Withdraw(amount);
                    _output.WriteLine(
                        $"Your withdrawal of {MoneyFormatter.Format(amount)} was successful. Your current balance is: {MoneyFormatter.Format(_wallet.Balance)}");
                    break;

                case CommandType.Bet:
                    PlayRound(amount);
                    break;

                case CommandType.Exit:
                    throw new InvalidOperationException("Exit command should be handled before command dispatch.");

                default:
                    throw new InvalidOperationException($"Unsupported command type: {command.Type}.");
            }
        }
        catch (WalletRuleException exception)
        {
            WriteRejected(exception.Message);
        }
    }

    private void PlayRound(decimal betAmount)
    {
        SlotGame.ValidateBetAmount(betAmount);

        if (betAmount > _wallet.Balance)
        {
            throw new WalletRuleException("Insufficient funds.");
        }

        var result = _game.Play(betAmount);
        _wallet.SettleBet(result.BetAmount, result.WinAmount);

        if (result.IsWin)
        {
            _output.WriteLine(
                $"Congrats - you won {MoneyFormatter.Format(result.WinAmount)}! Your current balance is: {MoneyFormatter.Format(_wallet.Balance)}");
            return;
        }

        _output.WriteLine($"No luck this time! Your current balance is: {MoneyFormatter.Format(_wallet.Balance)}");
    }

    private void WriteRejected(string reason)
    {
        _output.WriteLine($"{reason} Your current balance is: {MoneyFormatter.Format(_wallet.Balance)}");
    }
}
