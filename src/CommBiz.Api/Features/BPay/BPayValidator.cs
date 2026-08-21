namespace CommBiz.Api.Features.BPay;

// BPay field-level validation (F-016, docs/stash/BPay Payments - CommBiz File Specification.md
// §1.1/§1.3): mirrors DirectEntryValidator's style. Any field failure, anywhere in the batch, rejects
// the whole batch - no partial conversion.
public static class BPayValidator
{
    // Sentinel Index for batch-header-level errors (not tied to a specific instruction).
    private const int HeaderErrorIndex = -1;

    private const int MinimumInstructionCount = 1;

    // File Format Rules §1.1 rule 9: maximum 200 payments per file.
    private const int MaximumInstructionCount = 200;

    private const int MaxBillerCodeLength = 10;
    private const int MaxReferenceLength = 20;
    private const decimal MaxAmount = 9_999_999_999.99m;

    // File Format Rules §1.1 rule 6 (Payment Date, field 6): up to 15 months into the future from the
    // lodgement date. Treated as "from today" (rejecting past dates too), same window style as
    // ImtValidator/PriorityPaymentValidator's own PaymentDate checks.
    private const int MaxPaymentDateMonthsAhead = 15;

    public static IReadOnlyList<BPayInstructionError>? Validate(IReadOnlyList<BPayPaymentInstructionRequest> instructions)
    {
        List<BPayInstructionError>? errors = null;

        if (instructions.Count < MinimumInstructionCount)
        {
            errors ??= [];
            errors.Add(new BPayInstructionError(
                HeaderErrorIndex,
                $"Payment file must contain at least {MinimumInstructionCount} payment instruction(s) " +
                $"(found {instructions.Count})."));
        }

        if (instructions.Count > MaximumInstructionCount)
        {
            errors ??= [];
            errors.Add(new BPayInstructionError(
                HeaderErrorIndex,
                $"Payment file must contain at most {MaximumInstructionCount} payment instruction(s) " +
                $"(found {instructions.Count}), per the BPay file specification's 200-payment file limit."));
        }

        for (var index = 0; index < instructions.Count; index++)
        {
            foreach (var reason in ValidateInstruction(instructions[index]))
            {
                errors ??= [];
                errors.Add(new BPayInstructionError(index, reason));
            }
        }

        return errors;
    }

    private static IEnumerable<string> ValidateInstruction(BPayPaymentInstructionRequest instruction)
    {
        if (!IsValidNumericField(instruction.BPayBillerCode, MaxBillerCodeLength))
        {
            yield return $"BPayBillerCode '{instruction.BPayBillerCode}' must be numeric, 1-{MaxBillerCodeLength} digits.";
        }

        if (!IsValidNumericField(instruction.BPayReference, MaxReferenceLength))
        {
            yield return $"BPayReference '{instruction.BPayReference}' must be numeric, 1-{MaxReferenceLength} digits.";
        }

        if (instruction.Amount <= 0 || instruction.Amount > MaxAmount)
        {
            yield return $"Amount '{instruction.Amount}' must be positive and convert to at most 12 digits of cents.";
        }

        if (!IsWithinPaymentDateWindow(instruction.PaymentDate))
        {
            yield return $"PaymentDate '{instruction.PaymentDate:yyyy-MM-dd}' must be between today and " +
                $"{MaxPaymentDateMonthsAhead} months ahead.";
        }
    }

    private static bool IsValidNumericField(string value, int maxLength) =>
        !string.IsNullOrEmpty(value) && value.Length <= maxLength && value.All(char.IsDigit);

    private static bool IsWithinPaymentDateWindow(DateTime paymentDate)
    {
        var today = DateTime.UtcNow.Date;
        var processDate = paymentDate.Date;
        return processDate >= today && processDate <= today.AddMonths(MaxPaymentDateMonthsAhead);
    }
}
