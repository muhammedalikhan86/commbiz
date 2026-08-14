using System.Globalization;

namespace CommBiz.Api.Features.Imt;

// IMT field-level validation (F-017, docs/stash/CommBiz File Specification - International Money
// Transfers Priority Payments Non CBA Payment Requests (MT101) v9.md §1.2/§1.4): mirrors
// DirectEntryValidator/BPayValidator's style. Any field failure, anywhere in the batch, rejects the
// whole batch - no partial conversion.
public static class ImtValidator
{
    // Sentinel Index for batch-header-level errors (not tied to a specific instruction).
    private const int HeaderErrorIndex = -1;

    private const int MinimumInstructionCount = 1;

    // File Format Rules §1.2 rule 3: maximum 350 transactions per file (replaces BPAY's 200-row limit).
    private const int MaximumInstructionCount = 350;

    private const int MaxProcessDateDaysAhead = 7;
    private const int MaxBankNameLength = 30;
    private const int MaxBeneficiaryAccountNoLength = 34;
    private const int MaxBeneficiaryAccountNameLength = 62;
    private const int MaxBeneficiaryAddressLength = 40;
    private const int MaxPaymentDetailsLength = 105;

    // Fields 5/6: 1-11 digits before the decimal point, 1-2 after.
    private const decimal MaxAmount = 99_999_999_999.99m;

    public static IReadOnlyList<PaymentInstructionError>? Validate(IReadOnlyList<ImtPaymentInstructionRequest> instructions)
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
                $"(found {instructions.Count}), per the IMT file specification's 350-transaction file limit."));
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

    private static IEnumerable<string> ValidateInstruction(ImtPaymentInstructionRequest instruction)
    {
        if (string.IsNullOrWhiteSpace(instruction.Notes))
        {
            yield return "Notes (Transaction Description) must not be blank.";
        }

        if (!IsWithinProcessDateWindow(instruction.PaymentDate))
        {
            yield return $"PaymentDate '{instruction.PaymentDate:yyyy-MM-dd}' must be between today and " +
                $"today + {MaxProcessDateDaysAhead} days.";
        }

        if (!IsValidCurrencyCode(instruction.SourceCurrency))
        {
            yield return $"SourceCurrency '{instruction.SourceCurrency}' must be exactly 3 upper-case letters.";
        }

        if (!TryValidateAmounts(instruction.SourceAmount, instruction.Amount, out var amountReason))
        {
            yield return amountReason!;
        }

        if (!string.IsNullOrEmpty(instruction.IntermediaryBankSwiftCode) && !IsValidSwiftCode(instruction.IntermediaryBankSwiftCode))
        {
            yield return $"IntermediaryBankSwiftCode '{instruction.IntermediaryBankSwiftCode}' must be 8 or 11 alphanumeric characters.";
        }

        if (instruction.IntermediaryBankName is { Length: > MaxBankNameLength })
        {
            yield return $"IntermediaryBankName exceeds the maximum of {MaxBankNameLength} characters.";
        }

        if (!IsValidSwiftCode(instruction.DestinationBankSwiftCode))
        {
            yield return $"DestinationBankSwiftCode '{instruction.DestinationBankSwiftCode}' must be 8 or 11 alphanumeric characters.";
        }

        if (string.IsNullOrWhiteSpace(instruction.DestinationBankName) || instruction.DestinationBankName.Length > MaxBankNameLength)
        {
            yield return $"DestinationBankName must not be blank and must not exceed {MaxBankNameLength} characters.";
        }

        if (!IsValidBeneficiaryAccountNo(instruction.DestinationBankAccountNo))
        {
            yield return $"DestinationBankAccountNo '{instruction.DestinationBankAccountNo}' must be 1-{MaxBeneficiaryAccountNoLength} " +
                "characters and must not contain spaces, hyphens, or commas.";
        }

        if (!IsValidBeneficiaryAccountName(instruction.DestinationBankAccountName))
        {
            yield return $"DestinationBankAccountName '{instruction.DestinationBankAccountName}' must contain at least one letter, " +
                $"use only letters/digits/spaces/hyphens/apostrophes, and be at most {MaxBeneficiaryAccountNameLength} characters.";
        }

        if (string.IsNullOrWhiteSpace(instruction.BeneficiaryAddress))
        {
            yield return "BeneficiaryAddress must not be blank.";
        }
        else if (ImtRecordMapper.SanitizeFreeText(instruction.BeneficiaryAddress).Length > MaxBeneficiaryAddressLength)
        {
            yield return $"BeneficiaryAddress exceeds the maximum of {MaxBeneficiaryAddressLength} characters after sanitization.";
        }

        if (string.IsNullOrWhiteSpace(instruction.PaymentReference))
        {
            yield return "PaymentReference must not be blank.";
        }
        else if (ImtRecordMapper.SanitizeFreeText(instruction.PaymentReference).Length > MaxPaymentDetailsLength)
        {
            yield return $"PaymentReference exceeds the maximum of {MaxPaymentDetailsLength} characters after sanitization.";
        }
    }

    private static bool IsWithinProcessDateWindow(DateTime paymentDate)
    {
        var today = DateTime.UtcNow.Date;
        var processDate = paymentDate.Date;
        return processDate >= today && processDate <= today.AddDays(MaxProcessDateDaysAhead);
    }

    private static bool IsValidCurrencyCode(string currencyCode) =>
        currencyCode.Length == 3 && currencyCode.All(char.IsUpper);

    private static bool TryValidateAmounts(decimal sourceAmount, decimal amount, out string? reason)
    {
        var paymentAmountPopulated = sourceAmount > 0;
        var debitAmountPopulated = amount > 0;

        if (paymentAmountPopulated == debitAmountPopulated)
        {
            reason = paymentAmountPopulated
                ? "Exactly one of SourceAmount (Payment Amount) or Amount (Debit Amount) must be greater than zero, not both."
                : "Exactly one of SourceAmount (Payment Amount) or Amount (Debit Amount) must be greater than zero.";
            return false;
        }

        var populatedAmount = paymentAmountPopulated ? sourceAmount : amount;
        if (!IsValidAmountFormat(populatedAmount))
        {
            reason = $"The populated amount '{populatedAmount.ToString(CultureInfo.InvariantCulture)}' must be positive, " +
                "with at most 11 integer digits and 2 decimal digits.";
            return false;
        }

        reason = null;
        return true;
    }

    private static bool IsValidAmountFormat(decimal amount) =>
        amount > 0 && amount <= MaxAmount && Math.Round(amount, 2) == amount;

    private static bool IsValidSwiftCode(string swiftCode) =>
        swiftCode.Length is 8 or 11 && swiftCode.All(char.IsLetterOrDigit);

    private static bool IsValidBeneficiaryAccountNo(string accountNo) =>
        !string.IsNullOrEmpty(accountNo)
        && accountNo.Length <= MaxBeneficiaryAccountNoLength
        && !accountNo.Any(c => c is ' ' or '-' or ',');

    private static bool IsValidBeneficiaryAccountName(string accountName) =>
        !string.IsNullOrEmpty(accountName)
        && accountName.Length <= MaxBeneficiaryAccountNameLength
        && accountName.Any(char.IsLetter)
        && accountName.All(c => char.IsLetterOrDigit(c) || c is ' ' or '-' or '\'');
}
