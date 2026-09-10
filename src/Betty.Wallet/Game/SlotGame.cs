using Betty.Wallet;
using Betty.Wallet.Domain;

namespace Betty.Wallet.Game;

public sealed class SlotGame
{
    public const decimal MinimumBet = 1m;
    public const decimal MaximumBet = 10m;

    private readonly IRandomNumberGenerator _random;

    public SlotGame(IRandomNumberGenerator random)
    {
        _random = random;
    }

    public GameRoundResult Play(decimal betAmount)
    {
        ValidateBetAmount(betAmount);

        var roll = _random.Next(0, 100);

        return roll switch
        {
            < 50 => new GameRoundResult(betAmount, 0m, 0m, GameOutcome.Loss),
            < 90 => BuildWin(betAmount, GameOutcome.RegularWin, 100, 201),
            _ => BuildWin(betAmount, GameOutcome.BigWin, 201, 1001)
        };
    }

    public static void ValidateBetAmount(decimal betAmount)
    {
        MoneyRules.EnsurePositiveCurrencyAmount(betAmount, nameof(betAmount));

        if (betAmount is < MinimumBet or > MaximumBet)
        {
            throw new WalletRuleException(
                $"Bet amount must be between {MoneyFormatter.Format(MinimumBet)} and {MoneyFormatter.Format(MaximumBet)}.");
        }
    }

    private GameRoundResult BuildWin(decimal betAmount, GameOutcome outcome, int minMultiplierCents, int maxMultiplierCentsExclusive)
    {
        var multiplier = _random.Next(minMultiplierCents, maxMultiplierCentsExclusive) / 100m;
        var winAmount = MoneyRules.RoundToCents(betAmount * multiplier);

        return new GameRoundResult(betAmount, winAmount, multiplier, outcome);
    }
}
