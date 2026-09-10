# Code Review: Betty Wallet .NET Console Application

**Reviewed Files**: `README.md`, `Directory.Build.props`, `NuGet.config`, `Betty.Wallet.sln`, `src/Betty.Wallet/Betty.Wallet.csproj`, `src/Betty.Wallet/Program.cs`, `src/Betty.Wallet/ConsoleApp.cs`, `src/Betty.Wallet/CommandParser.cs`, `src/Betty.Wallet/CommandType.cs`, `src/Betty.Wallet/ParsedCommand.cs`, `src/Betty.Wallet/MoneyFormatter.cs`, `src/Betty.Wallet/Domain/MoneyRules.cs`, `src/Betty.Wallet/Domain/Wallet.cs`, `src/Betty.Wallet/Domain/WalletRuleException.cs`, `src/Betty.Wallet/Game/GameOutcome.cs`, `src/Betty.Wallet/Game/GameRoundResult.cs`, `src/Betty.Wallet/Game/IRandomNumberGenerator.cs`, `src/Betty.Wallet/Game/SlotGame.cs`, `src/Betty.Wallet/Game/SystemRandomNumberGenerator.cs`, `tests/Betty.Wallet.Tests/Betty.Wallet.Tests.csproj`, `tests/Betty.Wallet.Tests/ConsoleAppTests.cs`, `tests/Betty.Wallet.Tests/QueueRandomNumberGenerator.cs`, `tests/Betty.Wallet.Tests/SlotGameTests.cs`, `tests/Betty.Wallet.Tests/WalletTests.cs`
**Review Date**: September 9, 2026
**Severity Summary**: 0 Critical, 0 High, 2 Minor, 1 Nit

## Critical Issues

None found.

## High Priority Issues

None found.

## Minor Issues

### Very large valid deposits can crash the console app with `OverflowException`

- **Location**: `src/Betty.Wallet/Domain/Wallet.cs:11`, `src/Betty.Wallet/Domain/Wallet.cs:36`, `src/Betty.Wallet/ConsoleApp.cs:82`
- **Severity**: MINOR
- **Problem**: The wallet validates positivity and decimal precision, but it does not guard arithmetic overflow when adding deposits or settling bets. A user can enter a valid `decimal` amount near `decimal.MaxValue`, then run another deposit and crash the app. `ConsoleApp` only catches `WalletRuleException`, so the process terminates instead of rejecting the command and preserving the session.
- **Example**:

```csharp
Balance = MoneyRules.RoundToCents(Balance + amount);
```

Reproduction:

```text
deposit 79228162514264337593543950335
deposit 1
```

This produced:

```text
Unhandled exception. System.OverflowException: Value was either too large or too small for a Decimal.
```

- **Fix**: Convert arithmetic overflow into a wallet validation error, or define an explicit maximum balance/operation amount and reject values above it before arithmetic. Keep the check in the domain layer so direct `Wallet` usage and console usage behave the same.

```csharp
public void Deposit(decimal amount)
{
    MoneyRules.EnsurePositiveCurrencyAmount(amount, nameof(amount));

    try
    {
        Balance = MoneyRules.RoundToCents(Balance + amount);
    }
    catch (OverflowException exception)
    {
        throw new WalletRuleException("Amount is too large.", exception);
    }
}
```

Apply the same pattern, or a shared helper, to `SettleBet` because `Balance - betAmount + winAmount` can also overflow if the balance is already near the decimal limit.

- **Verification**: Add tests that deposit `decimal.MaxValue`, then attempt another deposit and assert `WalletRuleException` is thrown while the balance remains unchanged. Add a console test for the same scripted input and assert the app prints a rejection and continues to `exit`.
- **Prevention**: Add boundary tests for maximum accepted amounts and overflow paths whenever wallet arithmetic changes. If this assessment intentionally has a practical money limit, document it in `README.md` and enforce it in `MoneyRules`.

### Command parsing only treats spaces as separators

- **Location**: `src/Betty.Wallet/CommandParser.cs:20`
- **Severity**: MINOR
- **Problem**: The parser splits input only on the literal space character. Commands separated by tabs, such as `deposit\t10`, are rejected even though they are visually valid whitespace-delimited console input. This does not break the documented happy path, but it creates avoidable parsing surprises.
- **Example**:

```csharp
var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
```

- **Fix**: Split on all whitespace, for example with `input.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)` or a small tokenizer that uses `char.IsWhiteSpace`.
- **Verification**: Add parser or console tests for `deposit\t10`, `withdraw   5`, `bet\t1`, and `exit\t1` to confirm valid whitespace is accepted where appropriate and extra arguments are still rejected.
- **Prevention**: Keep command parser tests separate from end-to-end console tests so syntax edge cases are cheap to cover.

## Nits

### Money formatter suppresses cents in user-facing currency output

- **Location**: `src/Betty.Wallet/MoneyFormatter.cs:9`, `tests/Betty.Wallet.Tests/ConsoleAppTests.cs:36`
- **Severity**: NIT
- **Problem**: The domain enforces cent precision, but output uses `"0.##"`, so values like `$7.50` are displayed as `$7.5`. That is numerically correct and may be intentional because the README uses compact examples like `$0` and `$10`, but it is a possible output-contract mismatch if the assessment expects conventional two-decimal currency formatting.
- **Example**:

```csharp
return $"${amount.ToString("0.##", CultureInfo.InvariantCulture)}";
```

Current test expectation:

```text
Congrats - you won $7.5! Your current balance is: $12.5
```

- **Fix**: Keep the current formatter if compact output is acceptable. If the expected console format is standard currency, format all money with two decimal places.

```csharp
return $"${amount.ToString("0.00", CultureInfo.InvariantCulture)}";
```

- **Verification**: Add focused `MoneyFormatter` tests for `0m`, `7.5m`, `7.50m`, `10m`, and `10.01m`, then update console output assertions to match the chosen format.
- **Prevention**: Treat money display as an explicit output contract and test it independently from the console flow.

## Summary

The implementation is appropriately scoped for a technical assessment: the wallet starts at zero, positive amount and cent precision rules are centralized, withdrawals and bets cannot overdraft, bet limits are enforced, settlement uses `old balance - bet + win`, randomness is injectable for deterministic tests, and the README documents the payout bucket assumptions.

Security and concurrency risk are low for this single-threaded console app. The main correctness gap is unhandled decimal overflow from very large valid inputs, which can terminate the process. The remaining findings are smaller output/parsing risks that are worth tightening if the assessment grader checks exact console behavior beyond the included tests.

Verification performed: `dotnet test --no-restore` passed with 21 tests.

## Independent Second-Pass Audit

**Second-Pass Review Date**: September 9, 2026
**Second-Pass Severity Summary**: 0 Critical, 0 High, 2 Minor, 1 Nit
**Overall Verdict**: Confirmed after severity alignment. No additional issues found.

### Second-Pass Result

- Reproduced the overflow crash path and confirmed it is a real runtime edge case. Recommended Minor severity because it requires an extreme boundary value and is unlikely in normal assessment use.
- Confirmed the money formatter note as a Nit unless a strict two-decimal output contract is supplied.
- Confirmed the space-only parser issue as Minor.
- Found no additional correctness, security, runtime, concurrency, performance, or architecture issues.

### Areas Re-examined With No New Findings

- Wallet invariants for starting balance, positive amounts, cent precision, overdraft prevention, and bet settlement.
- Slot game payout bucket boundaries, multiplier ranges, injected randomness, and win rounding.
- Console command dispatch, rejection messages, EOF handling, and exit flow.
- Test coverage for wallet rules, game probabilities by boundary roll, deterministic randomness, and scripted console behavior.
- Project configuration, warning policy, package sources, solution structure, README assumptions, and ignored generated output.

### Second-Pass Summary

The implementation remains solid for the assessment requirements. The first review correctly caught the main edge cases, but I would not treat the decimal overflow as High severity and I would downgrade the formatter item to Nit unless a strict output format is supplied. No additional defects were found.

## Resolution Status

All findings from this review have been fixed.

- Decimal overflow during deposits and bet settlement is converted to `WalletRuleException`, leaving the balance unchanged.
- Command parsing now accepts any whitespace separator, including tabs.
- Money values now display with two decimal places.
- Regression coverage was added for all three findings.

Verification after fixes: `dotnet format Betty.Wallet.sln --verify-no-changes --no-restore`, `dotnet build Betty.Wallet.sln --configuration Release --no-restore`, and `dotnet test Betty.Wallet.sln --configuration Release --no-build` passed with 32 tests.
