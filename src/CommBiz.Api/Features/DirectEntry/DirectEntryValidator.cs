namespace CommBiz.Api.Features.DirectEntry;

// Direct Entry field-level validation (F-005, FR-002/FR-003, architecture.md §3): runs after the
// F-004 Payment Type Router, on instructions that already passed routing. Any field failure,
// anywhere in the batch, rejects the whole batch — no partial conversion.
public static class DirectEntryValidator
{
    // Sentinel Index for batch-header-level errors (not tied to a specific instruction).
    private const int HeaderErrorIndex = -1;

    // Direct Entry spec §1: "at least 2 detail records" per file.
    private const int MinimumInstructionCount = 2;

    private const int MaxFileNameLength = 26;
    private const int MaxUserIdentificationNumberLength = 6;
    private const int MaxDescriptionOfEntriesLength = 12;
    private const int MaxAccountNumberLength = 9;
    private const int MaxAccountTitleLength = 32;
    private const int MaxLodgementReferenceLength = 18;
    private const int MaxRemitterNameLength = 16;
    private const long MaxAmountInCents = 9_999_999_999;
    private const long MaxWithholdingTaxAmountInCents = 99_999_999;

    private static readonly string[] ValidIndicators = ["N", "W", "X", "Y", ""];
    private static readonly string[] ValidTransactionCodes =
        ["13", "50", "51", "52", "53", "54", "55", "56", "57"];

    public static IReadOnlyList<PaymentInstructionError>? Validate(ConvertDirectEntryBatchRequest request)
    {
        List<PaymentInstructionError>? errors = null;

        foreach (var reason in ValidateHeader(request))
        {
            errors ??= [];
            errors.Add(new PaymentInstructionError(HeaderErrorIndex, reason));
        }

        for (var index = 0; index < request.Instructions.Count; index++)
        {
            foreach (var reason in ValidateInstruction(request.Instructions[index]))
            {
                errors ??= [];
                errors.Add(new PaymentInstructionError(index, reason));
            }
        }

        return errors;
    }

    private static IEnumerable<string> ValidateHeader(ConvertDirectEntryBatchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FileName) || request.FileName.Length > MaxFileNameLength)
        {
            yield return $"FileName must not be blank and must be at most {MaxFileNameLength} characters.";
        }

        if (!IsDigitsOnly(request.UserIdentificationNumber) ||
            request.UserIdentificationNumber.Length > MaxUserIdentificationNumberLength)
        {
            yield return
                $"UserIdentificationNumber must be numeric and at most {MaxUserIdentificationNumberLength} digits.";
        }

        if (string.IsNullOrWhiteSpace(request.DescriptionOfEntries) ||
            request.DescriptionOfEntries.Length > MaxDescriptionOfEntriesLength)
        {
            yield return
                $"DescriptionOfEntries must not be blank and must be at most {MaxDescriptionOfEntriesLength} characters.";
        }

        if (request.Instructions.Count < MinimumInstructionCount)
        {
            yield return
                $"Payment file must contain at least {MinimumInstructionCount} detail records " +
                $"(found {request.Instructions.Count}).";
        }
    }

    private static IEnumerable<string> ValidateInstruction(PaymentInstructionRequest instruction)
    {
        if (!IsValidBsb(instruction.Bsb))
        {
            yield return $"Bsb '{instruction.Bsb}' must match format nnn-nnn.";
        }

        if (!IsValidBsb(instruction.TraceBsb))
        {
            yield return $"TraceBsb '{instruction.TraceBsb}' must match format nnn-nnn.";
        }

        if (!IsValidAccountNumber(instruction.AccountNumber))
        {
            yield return $"AccountNumber '{instruction.AccountNumber}' is invalid.";
        }

        if (!IsValidAccountNumber(instruction.TraceAccountNumber))
        {
            yield return $"TraceAccountNumber '{instruction.TraceAccountNumber}' is invalid.";
        }

        if (!ValidIndicators.Contains(instruction.Indicator))
        {
            yield return $"Indicator '{instruction.Indicator}' must be one of N, W, X, Y or blank.";
        }

        if (!ValidTransactionCodes.Contains(instruction.TransactionCode))
        {
            yield return $"TransactionCode '{instruction.TransactionCode}' is not a supported code.";
        }

        if (instruction.AmountInCents < 0 || instruction.AmountInCents > MaxAmountInCents)
        {
            yield return $"AmountInCents '{instruction.AmountInCents}' must be non-negative and at most 10 digits.";
        }

        if (string.IsNullOrWhiteSpace(instruction.AccountTitle) ||
            instruction.AccountTitle.Length > MaxAccountTitleLength)
        {
            yield return $"AccountTitle must not be blank and must be at most {MaxAccountTitleLength} characters.";
        }

        if (instruction.LodgementReference.Length > MaxLodgementReferenceLength)
        {
            yield return $"LodgementReference must be at most {MaxLodgementReferenceLength} characters.";
        }

        if (string.IsNullOrWhiteSpace(instruction.RemitterName) ||
            instruction.RemitterName.Length > MaxRemitterNameLength)
        {
            yield return $"RemitterName must not be blank and must be at most {MaxRemitterNameLength} characters.";
        }

        if (instruction.WithholdingTaxAmountInCents < 0 ||
            instruction.WithholdingTaxAmountInCents > MaxWithholdingTaxAmountInCents)
        {
            yield return "WithholdingTaxAmountInCents must be non-negative and at most 8 digits.";
        }
    }

    private static bool IsValidBsb(string bsb) =>
        bsb.Length == 7 &&
        bsb[3] == '-' &&
        char.IsDigit(bsb[0]) && char.IsDigit(bsb[1]) && char.IsDigit(bsb[2]) &&
        char.IsDigit(bsb[4]) && char.IsDigit(bsb[5]) && char.IsDigit(bsb[6]);

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

    private static bool IsDigitsOnly(string value) => value.Length > 0 && value.All(char.IsDigit);
}
