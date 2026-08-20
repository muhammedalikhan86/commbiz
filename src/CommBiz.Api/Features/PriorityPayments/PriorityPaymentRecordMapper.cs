using System.Globalization;
using System.Text.RegularExpressions;
using CommBiz.Api.Features.Shared;

namespace CommBiz.Api.Features.PriorityPayments;

// Priority Payments record mapping (F-018, docs/stash/CommBiz File Specification - International
// Money Transfers Priority Payments Non CBA Payment Requests (MT101) v9.md §1.2/§1.5): manual field
// concatenation only, per ADR-004 (no AutoMapper). CSV, comma-delimited, 27 fields, same shape as IMT
// but almost entirely domestic/BSB-based - most SWIFT/currency/intermediary fields are always blank.
public static partial class PriorityPaymentRecordMapper
{
    private const string TransactionType = "PP"; // field 1 constant - never "RTGS" (Shaw and Partners' routing code)

    // Field 2: Transaction Description, up to 12AN - truncated (not rejected) if longer, never padded
    // since this is CSV, not fixed-width.
    private const int MaxTransactionDescriptionLength = 12;

    // Field 20 is stricter than IMT's address field: letters/digits/spaces only, no hyphen/apostrophe -
    // do not reuse ImtRecordMapper.SanitizeFreeText, which incorrectly permits both for this field.
    [GeneratedRegex("[^A-Za-z0-9 ]")]
    private static partial Regex DisallowedAddressCharsRegex();

    [GeneratedRegex(" {2,}")]
    private static partial Regex RepeatedSpacesRegex();

    public static string Map(PriorityPaymentInstructionRequest instruction, string debitAccountNumber) =>
        string.Join(
            ",",
            TransactionType, // 1: Transaction Type
            TruncateTransactionDescription(instruction.Notes), // 2: Transaction Description
            instruction.PaymentDate.ToString("yyMMdd", CultureInfo.InvariantCulture), // 3: Process Date
            "", // 4: Payment Currency - not applicable
            instruction.Amount.ToString(CultureInfo.InvariantCulture), // 5: Payment Amount
            "", // 6: Debit Amount - not applicable
            debitAccountNumber, // 7: Debit Account - Account Number
            "", // 8: Dealer Code - not applicable
            "", // 9: Dealer Exchange Rate - not applicable
            "", // 10: Intermediary Bank - Bank Code - not applicable
            "", // 11: Intermediary Bank - Name - not applicable
            "", // 12: Intermediary Bank - City - not applicable
            "", // 13: Intermediary Institution - Country - not applicable
            instruction.DestinationBankBsb, // 14: Beneficiary Bank - Bank Code (plain 6-digit BSB, no hyphen)
            "", // 15: Beneficiary Bank - Name - not applicable
            "", // 16: Beneficiary Bank - City - not applicable
            "", // 17: Beneficiary Bank - Country - not applicable
            instruction.DestinationBankAccountNo, // 18: Beneficiary - Account Number
            instruction.DestinationBankAccountName, // 19: Beneficiary - Account Name
            SanitizeAddress(instruction.BeneficiaryAddress), // 20: Beneficiary - Address line 1
            "", // 21: Beneficiary - Address line 2 - no second address line in the payload
            "", // 22: Beneficiary - Address line 3 - not applicable
            "", // 23: Beneficiary - City - no discrete field in the payload
            "", // 24: Beneficiary - State - not applicable
            "", // 25: Beneficiary - Postcode - not applicable
            "", // 26: Beneficiary - Country Code - no discrete field in the payload
            ""); // 27: Beneficiary Payment Details - Notes already maps to field 2

    // One entry per Priority Payments CSV field position (27 total, same order as Map), including
    // not-applicable positions - a dropped position would break correspondence between Fields and the
    // raw comma-separated output line (the F-021 correction already applied to IMT/BPay/DE).
    public static IReadOnlyList<FieldMapping> MapFields(PriorityPaymentInstructionRequest instruction, string debitAccountNumber) =>
        [
            new(nameof(TransactionType), TransactionType, "Transaction Type", TransactionType),
            new(
                nameof(instruction.Notes),
                instruction.Notes,
                "Transaction Description",
                TruncateTransactionDescription(instruction.Notes)),
            new(
                nameof(instruction.PaymentDate),
                instruction.PaymentDate.ToString("O"),
                "Process Date",
                instruction.PaymentDate.ToString("yyMMdd", CultureInfo.InvariantCulture)),
            new("", "", "Payment Currency", ""),
            new(
                nameof(instruction.Amount),
                instruction.Amount.ToString(CultureInfo.InvariantCulture),
                "Payment Amount",
                instruction.Amount.ToString(CultureInfo.InvariantCulture)),
            new("", "", "Debit Amount", ""),
            new("DebitAccountBsb+DebitAccountNumber", debitAccountNumber, "Debit Account - Account Number", debitAccountNumber),
            new("", "", "Dealer Code", ""),
            new("", "", "Dealer Exchange Rate", ""),
            new("", "", "Intermediary Bank - Bank Code", ""),
            new("", "", "Intermediary Bank - Name", ""),
            new("", "", "Intermediary Bank - City", ""),
            new("", "", "Intermediary Institution - Country", ""),
            new(
                nameof(instruction.DestinationBankBsb),
                instruction.DestinationBankBsb,
                "Beneficiary Bank - Bank Code",
                instruction.DestinationBankBsb),
            new("", "", "Beneficiary Bank - Name", ""),
            new("", "", "Beneficiary Bank - City", ""),
            new("", "", "Beneficiary Bank - Country", ""),
            new(
                nameof(instruction.DestinationBankAccountNo),
                instruction.DestinationBankAccountNo,
                "Beneficiary - Account Number",
                instruction.DestinationBankAccountNo),
            new(
                nameof(instruction.DestinationBankAccountName),
                instruction.DestinationBankAccountName,
                "Beneficiary - Account Name",
                instruction.DestinationBankAccountName),
            new(
                nameof(instruction.BeneficiaryAddress),
                instruction.BeneficiaryAddress,
                "Beneficiary - Address line 1",
                SanitizeAddress(instruction.BeneficiaryAddress)),
            new("", "", "Beneficiary - Address line 2", ""),
            new("", "", "Beneficiary - Address line 3", ""),
            new("", "", "Beneficiary - City", ""),
            new("", "", "Beneficiary - State", ""),
            new("", "", "Beneficiary - Postcode", ""),
            new("", "", "Beneficiary - Country Code", ""),
            new("", "", "Beneficiary Payment Details", ""),
        ];

    // Field 7: last 4 digits of the static PriorityPaymentsSettings BSB (hyphens stripped) + the
    // static account number (spaces stripped) - same derivation as ImtRecordMapper.DeriveDebitAccountNumber.
    public static string DeriveDebitAccountNumber(PriorityPaymentsSettings settings)
    {
        var bsbDigits = settings.DebitAccountBsb.Replace("-", "");
        var last4 = bsbDigits.Length > 4 ? bsbDigits[^4..] : bsbDigits;
        var accountDigits = settings.DebitAccountNumber.Replace(" ", "");
        return last4 + accountDigits;
    }

    // Field 20: replace disallowed characters (anything other than letters/digits/space, per the
    // stricter PP address rule) with a space, then collapse repeated spaces - never drop characters
    // silently by deleting them outright, so an address with punctuation doesn't fuse two words together.
    public static string SanitizeAddress(string? value) =>
        value is null ? "" : RepeatedSpacesRegex().Replace(DisallowedAddressCharsRegex().Replace(value, " "), " ").Trim();

    public static string TruncateTransactionDescription(string value) =>
        value.Length > MaxTransactionDescriptionLength ? value[..MaxTransactionDescriptionLength] : value;
}
