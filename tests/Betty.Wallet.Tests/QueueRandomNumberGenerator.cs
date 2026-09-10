using Betty.Wallet.Game;

namespace Betty.Wallet.Tests;

public sealed class QueueRandomNumberGenerator : IRandomNumberGenerator
{
    private readonly Queue<int> _values;

    public QueueRandomNumberGenerator(params int[] values)
    {
        _values = new Queue<int>(values);
    }

    public int Next(int minInclusive, int maxExclusive)
    {
        var value = _values.Dequeue();

        Assert.InRange(value, minInclusive, maxExclusive - 1);

        return value;
    }
}
