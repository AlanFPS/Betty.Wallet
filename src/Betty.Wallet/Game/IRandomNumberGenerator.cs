namespace Betty.Wallet.Game;

public interface IRandomNumberGenerator
{
    int Next(int minInclusive, int maxExclusive);
}
