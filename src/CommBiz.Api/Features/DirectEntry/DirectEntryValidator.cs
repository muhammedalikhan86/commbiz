namespace CommBiz.Api.Features.DirectEntry;

// Direct Entry field-level validation (F-005, FR-002/FR-003, architecture.md §3): runs on instructions
// already guaranteed to be "DE"-only by the top-level Payment Type Router (Features/PaymentRouting).
// Any field failure, anywhere in the batch, rejects the whole batch — no partial conversion.
public static class DirectEntryValidator
{
    // Sentinel Index for batch-header-level errors (not tied to a specific instruction).
    private const int HeaderErrorIndex = -1;

    // Direct Entry spec §1 requires >=2 detail records per file; the self-balancing record (F-014) now
    // guarantees that structurally, so the request-level minimum drops to 1 payment instruction.
    private const int MinimumInstructionCount = 1;

    private const int MaxAccountNumberLength = 9;
    private const decimal MaxAmount = 99_999_999.99m;

    public static IReadOnlyList<PaymentInstructionError>? Validate(IReadOnlyList<PaymentInstructionRequest> instructions)
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

    private static IEnumerable<string> ValidateInstruction(PaymentInstructionRequest instruction)
    {
        if (string.IsNullOrWhiteSpace(instruction.AccountNo))
        {
            yield return "AccountNo must not be blank.";
        }

        if (!IsValidBsb(instruction.SourceBankBsb))
        {
            yield return $"SourceBankBsb '{instruction.SourceBankBsb}' must be exactly 6 numeric digits.";
        }

        if (!IsValidAccountNumber(instruction.SourceBankAccountNo))
        {
            yield return $"SourceBankAccountNo '{instruction.SourceBankAccountNo}' is invalid.";
        }

        if (!IsValidBsb(instruction.DestinationBankBsb))
        {
            yield return $"DestinationBankBsb '{instruction.DestinationBankBsb}' must be exactly 6 numeric digits.";
        }

        if (!IsValidAccountNumber(instruction.DestinationBankAccountNo))
        {
            yield return $"DestinationBankAccountNo '{instruction.DestinationBankAccountNo}' is invalid.";
        }

        if (string.IsNullOrWhiteSpace(instruction.DestinationBankAccountName))
        {
            yield return "DestinationBankAccountName must not be blank.";
        }

        if (instruction.Amount <= 0 || instruction.Amount > MaxAmount)
        {
            yield return $"Amount '{instruction.Amount}' must be positive and convert to at most 10 digits of cents.";
        }
    }

    private static bool IsValidBsb(string bsb) =>
        bsb.Length == 6 && bsb.All(char.IsDigit);

    private static bool IsValidAccountNumber(string accountNumber)
    {
        if (string.IsNullOrEmpty(accountNumber) || accountNumber.Length > MaxAccountNumberLength)
        {
            return false;
        }

        if (!accountNumber.All(c => char.IsLetterOrDigit(c) || c is '-' or ' '))
        {
            return false;
        }

        var significantChars = accountNumber.Replace("-", string.Empty).Replace(" ", string.Empty);
        if (significantChars.Length == 0)
        {
            return false; // all blank (ignoring hyphens/spaces)
        }

        if (significantChars.All(c => c == '0'))
        {
            return false; // all zeros
        }

        return true;
    }
}
