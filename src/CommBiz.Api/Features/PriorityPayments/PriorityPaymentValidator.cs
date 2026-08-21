using System.Globalization;
using System.Text.RegularExpressions;

namespace CommBiz.Api.Features.PriorityPayments;

// Priority Payments field-level validation (F-018, docs/stash/CommBiz File Specification -
// International Money Transfers Priority Payments Non CBA Payment Requests (MT101) v9.md §1.2/§1.5):
// mirrors ImtValidator's style. Any field failure, anywhere in the batch, rejects the whole batch - no
// partial conversion. SourceBankBsb/SourceBankAccountNo/SourceBankAccountName/SourceCurrency/
// SourceAmount are never validated - unused, same as IMT's unused fields.
public static partial class PriorityPaymentValidator
{
    // Sentinel index for batch-header-level errors (not tied to a specific instruction).
    private const int HeaderErrorIndex = -1;

    private const int MinimumInstructionCount = 1;

    // File Format Rules §1.2 rule 3: maximum 350 transactions per file - shared across IMT/PP/NonCBA.
    private const int MaximumInstructionCount = 350;

    private const int MaxProcessDateMonthsAhead = 14;
    private const int MaxBeneficiaryAccountNameLength = 32;
    private const int MaxBeneficiaryAddressLength = 40;
    private const int MinBeneficiaryAccountNoLength = 3;
    private const int MaxBeneficiaryAccountNoLength = 9;

    // Field 5: 1-11 digits before the decimal point, 1-2 after.
    private const decimal MaxAmount = 99_999_999_999.99m;

    [GeneratedRegex("^[A-Za-z0-9 ]*$")]
    private static partial Regex LettersDigitsSpacesRegex();

    [GeneratedRegex("^[0-9]{6}$")]
    private static partial Regex SixDigitBsbRegex();

    [GeneratedRegex("^[A-Za-z0-9]{3,9}$")]
    private static partial Regex AccountNoRegex();

    public static IReadOnlyList<PaymentInstructionError>? Validate(IReadOnlyList<PriorityPaymentInstructionRequest> instructions)
    {
        List<PaymentInstructionError>? errors = null;

        if (instructions.Count < MinimumInstructionCount)
        {
            errors ??= [];
            errors.Add(new PaymentInstructionError(
                HeaderErrorIndex,
                $"Payment file must contain at least {MinimumInstructionCount} payment instruction(s) " +
                $"(found {instructions.Count})."));
        }

        if (instructions.Count > MaximumInstructionCount)
        {
            errors ??= [];
            errors.Add(new PaymentInstructionError(
                HeaderErrorIndex,
                $"Payment file must contain at most {MaximumInstructionCount} payment instruction(s) " +
                $"(found {instructions.Count}), per the shared IMT/PP/NonCBA 350-transaction file limit."));
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

    private static IEnumerable<string> ValidateInstruction(PriorityPaymentInstructionRequest instruction)
    {
        if (string.IsNullOrWhiteSpace(instruction.Notes))
        {
            yield return "Notes (Transaction Description) must not be blank.";
        }

        if (!IsWithinProcessDateWindow(instruction.PaymentDate))
        {
            yield return $"PaymentDate '{instruction.PaymentDate:yyyy-MM-dd}' must be between today and " +
                $"{MaxProcessDateMonthsAhead} months ahead.";
        }

        if (!IsValidAmountFormat(instruction.Amount))
        {
            yield return $"Amount '{instruction.Amount.ToString(CultureInfo.InvariantCulture)}' must be greater than zero, " +
                "with at most 11 integer digits and 2 decimal digits.";
        }

        if (!SixDigitBsbRegex().IsMatch(instruction.DestinationBankBsb ?? ""))
        {
            yield return $"DestinationBankBsb '{instruction.DestinationBankBsb}' must be exactly 6 numeric digits.";
        }

        if (!AccountNoRegex().IsMatch(instruction.DestinationBankAccountNo ?? ""))
        {
            yield return $"DestinationBankAccountNo '{instruction.DestinationBankAccountNo}' must be " +
                $"{MinBeneficiaryAccountNoLength}-{MaxBeneficiaryAccountNoLength} alphanumeric characters.";
        }

        if (!IsValidBeneficiaryAccountName(instruction.DestinationBankAccountName))
        {
            yield return $"DestinationBankAccountName '{instruction.DestinationBankAccountName}' must not be blank, " +
                $"must contain only letters/numbers/spaces, and be at most {MaxBeneficiaryAccountNameLength} characters.";
        }

        if (!IsValidBeneficiaryAddress(instruction.BeneficiaryAddress))
        {
            yield return $"BeneficiaryAddress '{instruction.BeneficiaryAddress}' must be at most " +
                $"{MaxBeneficiaryAddressLength} characters and contain only letters/numbers/spaces.";
        }
    }

    private static bool IsWithinProcessDateWindow(DateTime paymentDate)
    {
        var today = DateTime.UtcNow.Date;
        var processDate = paymentDate.Date;
        return processDate >= today && processDate <= today.AddMonths(MaxProcessDateMonthsAhead);
    }

    private static bool IsValidAmountFormat(decimal amount) =>
        amount > 0 && amount <= MaxAmount && Math.Round(amount, 2) == amount;

    private static bool IsValidBeneficiaryAccountName(string accountName) =>
        !string.IsNullOrWhiteSpace(accountName)
        && accountName.Length <= MaxBeneficiaryAccountNameLength
        && LettersDigitsSpacesRegex().IsMatch(accountName);

    // Optional field: null/blank is valid (unlike IMT's mandatory beneficiary address). When present,
    // it must satisfy the stricter PP character rule and length bound.
    private static bool IsValidBeneficiaryAddress(string? address) =>
        string.IsNullOrWhiteSpace(address)
        || (address.Length <= MaxBeneficiaryAddressLength && LettersDigitsSpacesRegex().IsMatch(address));
}
