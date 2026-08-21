using System.Globalization;
using System.Text.RegularExpressions;

namespace CommBiz.Api.Features.Fx;

// FX field-level validation (F-023, docs/stash/CommBiz IPFX Bulk Settlement Upload - File
// Specification v4.0 2.md "File Description and Business Rules" / "File Contents - Data Rows Format"):
// mirrors PriorityPaymentValidator's style. Any field failure, anywhere in the batch, rejects the
// whole batch - no partial conversion. PaymentDate/Notes/RateTypeCode/ValueDateTypeCode/FeeTypeCode/
// FeeOtherTypeCode are never validated - unused, same as IMT/PP's unused fields.
public static partial class FxValidator
{
    // Sentinel index for batch-header-level errors (not tied to a specific instruction).
    private const int HeaderErrorIndex = -1;

    private const int MinimumInstructionCount = 1;

    // File Description and Business Rules, rule 3: file must contain between 1 and 200 rows of data.
    private const int MaximumInstructionCount = 200;

    // Rule 5: Commbiz Markets Bulk Settlement Upload allows up to 15 distinct currency pairs per file.
    private const int MaximumCurrencyPairCount = 15;

    // Field 2: Transaction Description, up to 12AN.
    private const int MaxAccountNoLength = 12;

    // Fields 4/6: 1-11 digits before the decimal point, 1-2 after.
    private const decimal MaxAmount = 99_999_999_999.99m;

    [GeneratedRegex("^[A-Z]{3}$")]
    private static partial Regex CurrencyCodeRegex();

    [GeneratedRegex("^[A-Za-z0-9]{1,12}$")]
    private static partial Regex AccountNoRegex();

    public static IReadOnlyList<PaymentInstructionError>? Validate(IReadOnlyList<FxPaymentInstructionRequest> instructions)
    {
        List<PaymentInstructionError>? errors = null;

        if (instructions.Count < MinimumInstructionCount)
        {
            errors ??= [];
            errors.Add(new PaymentInstructionError(
                HeaderErrorIndex,
                $"FX file must contain at least {MinimumInstructionCount} payment instruction(s) " +
                $"(found {instructions.Count})."));
        }

        if (instructions.Count > MaximumInstructionCount)
        {
            errors ??= [];
            errors.Add(new PaymentInstructionError(
                HeaderErrorIndex,
                $"FX file must contain at most {MaximumInstructionCount} payment instruction(s) " +
                $"(found {instructions.Count})."));
        }

        var distinctCurrencyPairCount = instructions
            .Select(instruction => (instruction.BuyCurrency, instruction.SellCurrency))
            .Distinct()
            .Count();

        if (distinctCurrencyPairCount > MaximumCurrencyPairCount)
        {
            errors ??= [];
            errors.Add(new PaymentInstructionError(
                HeaderErrorIndex,
                $"FX file must settle at most {MaximumCurrencyPairCount} distinct currency pairs " +
                $"(found {distinctCurrencyPairCount})."));
        }

        for (var index = 0; index < instructions.Count; index++)
        {
            foreach (var reason in ValidateInstruction(instructions[index]))
            {
                errors ??= [];
                errors.Add(new PaymentInstructionError(index, reason));
            }
        }

        return errors;
    }

    private static IEnumerable<string> ValidateInstruction(FxPaymentInstructionRequest instruction)
    {
        if (!CurrencyCodeRegex().IsMatch(instruction.BuyCurrency ?? ""))
        {
            yield return $"BuyCurrency '{instruction.BuyCurrency}' must be exactly 3 uppercase alphabetic characters.";
        }

        if (!CurrencyCodeRegex().IsMatch(instruction.SellCurrency ?? ""))
        {
            yield return $"SellCurrency '{instruction.SellCurrency}' must be exactly 3 uppercase alphabetic characters.";
        }

        if (!IsValidAmountFormat(instruction.Amount))
        {
            yield return $"Amount '{instruction.Amount.ToString(CultureInfo.InvariantCulture)}' must be greater than zero, " +
                "with at most 11 integer digits and 2 decimal digits.";
        }

        if (!AccountNoRegex().IsMatch(instruction.AccountNo ?? ""))
        {
            yield return $"AccountNo '{instruction.AccountNo}' must be 1-{MaxAccountNoLength} alphanumeric characters.";
        }
    }

    private static bool IsValidAmountFormat(decimal amount) =>
        amount > 0 && amount <= MaxAmount && Math.Round(amount, 2) == amount;
}
