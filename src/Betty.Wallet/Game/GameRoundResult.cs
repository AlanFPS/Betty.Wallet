namespace Betty.Wallet.Game;

public sealed record GameRoundResult(
    decimal BetAmount,
    decimal WinAmount,
    decimal Multiplier,
    GameOutcome Outcome)
{
    public bool IsWin => WinAmount > 0;
}
