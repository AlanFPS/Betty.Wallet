# Betty Wallet

A .NET 8 console application that simulates a simple player wallet with deposits, withdrawals, and slot-style betting.

## Run

```bash
dotnet run --project src/Betty.Wallet
```

## Test

```bash
dotnet test
```

## Commands

- `deposit <amount>` adds funds to the wallet.
- `withdraw <amount>` removes funds when the balance is sufficient.
- `bet <amount>` plays one game round.
- `exit` ends the session.

Amounts must be positive numbers with no more than two decimal places. The wallet starts at `$0.00`, and every accepted or rejected operation prints the current balance.

## Game Rules

Bets must be between `$1.00` and `$10.00`.

Each round settles the wallet with:

```text
new balance = old balance - bet amount + win amount
```

The payout buckets are:

- 50% loss, no win amount.
- 40% regular win, between `1.00x` and `2.00x` the bet.
- 10% big win, between `2.01x` and `10.00x` the bet.

The task says regular wins pay "up to x2" and big wins pay "between x2 and x10". I used `1.00x` as the lower regular-win multiplier and `2.01x` as the lower big-win multiplier so the buckets do not overlap.

## Design Notes

The wallet rules live in `Betty.Wallet.Domain`, the game rules live in `Betty.Wallet.Game`, and the console loop only handles input and output. That keeps money validation and round settlement testable without running the interactive app.

The game takes an `IRandomNumberGenerator`, which lets tests force exact rolls and multiplier values. The production app uses `System.Random`, which is fine for this simulation but would not be suitable for a regulated real-money casino game.

Currency is represented with `decimal`, rounded to cents after calculations, and displayed with two decimal places.
