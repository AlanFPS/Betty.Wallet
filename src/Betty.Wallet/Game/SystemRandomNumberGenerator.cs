namespace Betty.Wallet.Game;

public sealed class SystemRandomNumberGenerator : IRandomNumberGenerator
{
    private readonly Random _random = new();

    public int Next(int minInclusive, int maxExclusive)
    {
        return _random.Next(minInclusive, maxExclusive);
    }
}
