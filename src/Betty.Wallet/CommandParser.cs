using System.Globalization;

namespace Betty.Wallet;

public sealed class CommandParser
{
    private const string CommandHint = "Use deposit <amount>, withdraw <amount>, bet <amount>, or exit.";

    public bool TryParse(string? input, out ParsedCommand command, out string error)
    {
        command = new ParsedCommand(CommandType.Exit, null);
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = CommandHint;
            return false;
        }

        var parts = input.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var verb = parts[0].ToLowerInvariant();

        if (verb == "exit")
        {
            if (parts.Length != 1)
            {
                error = "Exit does not accept an amount.";
                return false;
            }

            command = new ParsedCommand(CommandType.Exit, null);
            return true;
        }

        CommandType? commandType = verb switch
        {
            "deposit" => CommandType.Deposit,
            "withdraw" => CommandType.Withdraw,
            "bet" => CommandType.Bet,
            _ => null
        };

        if (commandType is null)
        {
            return UnknownCommand(verb, out error);
        }

        if (parts.Length != 2)
        {
            error = $"The {verb} command requires exactly one amount. {CommandHint}";
            return false;
        }

        if (!decimal.TryParse(parts[1], NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
        {
            error = "Amount must be a positive number.";
            return false;
        }

        return Parsed(commandType.Value, amount, out command);
    }

    private static bool Parsed(CommandType type, decimal amount, out ParsedCommand command)
    {
        command = new ParsedCommand(type, amount);
        return true;
    }

    private static bool UnknownCommand(string verb, out string error)
    {
        error = $"Unknown command '{verb}'. {CommandHint}";
        return false;
    }
}
