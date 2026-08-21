using System.Globalization;
using System.Text.RegularExpressions;
using CommBiz.Api.Features.Shared;
using static CommBiz.Api.Features.Shared.MappingUtilities;

namespace CommBiz.Api.Features.Imt;

// IMT record mapping (F-017, docs/stash/CommBiz File Specification - International Money Transfers
// Priority Payments Non CBA Payment Requests (MT101) v9.md §1.2/§1.4): manual field concatenation
// only, per ADR-004 (no AutoMapper). CSV, comma-delimited, 27 fields, NOT fixed-width/padded.
// SanitizeFreeText is shared with ImtValidator so sanitization is computed with one rule, not
// duplicated between validate and map. DeriveCountryFromSwift now lives in Shared/MappingUtilities
// (shared with FX, no longer a per-slice copy).
public static partial class ImtRecordMapper
{
    private const string TransactionType = "IMT"; // field 1 constant - never "TT" (the API's routing code)

    // Field 2: Transaction Description, up to 12AN - truncated (not rejected) if longer, never padded
    // since this is CSV, not fixed-width.
    private const int MaxTransactionDescriptionLength = 12;

    [GeneratedRegex("[^A-Za-z0-9 '-]")]
    private static partial Regex DisallowedFreeTextCharsRegex();

    [GeneratedRegex(" {2,}")]
    private static partial Regex RepeatedSpacesRegex();

    public static string Map(ImtPaymentInstructionRequest instruction, string debitAccountNumber)
    {
        // Derived once per instruction, used by both the intermediary (13) and beneficiary (17/26) fields.
        var intermediaryCountry = DeriveCountryFromSwift(instruction.IntermediaryBankSwiftCode);
        var beneficiaryCountry = DeriveCountryFromSwift(instruction.DestinationBankSwiftCode);

        return string.Join(
            ",",
            TransactionType, // 1: Transaction Type
            Truncate(instruction.Notes, MaxTransactionDescriptionLength), // 2: Transaction Description
            instruction.PaymentDate.ToString("yyMMdd", CultureInfo.InvariantCulture), // 3: Process Date
            instruction.SourceCurrency, // 4: Payment Currency
            FormatAmount(instruction.SourceAmount), // 5: Payment Amount
            FormatAmount(instruction.Amount), // 6: Debit Amount
            debitAccountNumber, // 7: Debit Account - Account Number
            "", // 8: Dealer Code - not present in payload
            "", // 9: Dealer Exchange Rate - not present in payload
            instruction.IntermediaryBankSwiftCode ?? "", // 10: Intermediary Bank - Bank Code
            instruction.IntermediaryBankName ?? "", // 11: Intermediary Bank - Name
            "", // 12: Intermediary Bank - City - no discrete city field
            intermediaryCountry, // 13: Intermediary Institution - Country
            instruction.DestinationBankSwiftCode, // 14: Beneficiary Bank - Bank Code
            instruction.DestinationBankName, // 15: Beneficiary Bank - Name
            "", // 16: Beneficiary Bank - City - no discrete city field
            beneficiaryCountry, // 17: Beneficiary Bank - Country
            instruction.DestinationBankAccountNo, // 18: Beneficiary - Account Number
            instruction.DestinationBankAccountName, // 19: Beneficiary - Account Name
            SanitizeFreeText(instruction.BeneficiaryAddress), // 20: Beneficiary - Address line 1
            "", // 21: Reserved for future use - never populated, rejected by CommBiz otherwise
            "", // 22: Reserved for future use - never populated, rejected by CommBiz otherwise
            "", // 23: Beneficiary - City - no discrete field available
            "", // 24: Beneficiary - State - no discrete field available
            "", // 25: Beneficiary - Postcode - no discrete field available
            beneficiaryCountry, // 26: Beneficiary - Country (same derivation as 17)
            SanitizeFreeText(instruction.PaymentReference)); // 27: Beneficiary Payment Details
    }

    // F-021 correction: one entry per IMT CSV field position (27 total, same order as Map), including
    // reserved/unused fields 8/9/12/16/21-25 - a dropped position would break correspondence between
    // Fields and the raw comma-separated output line.
    public static IReadOnlyList<FieldMapping> MapFields(ImtPaymentInstructionRequest instruction, string debitAccountNumber)
    {
        var intermediaryCountry = DeriveCountryFromSwift(instruction.IntermediaryBankSwiftCode);
        var beneficiaryCountry = DeriveCountryFromSwift(instruction.DestinationBankSwiftCode);

        return
        [
            new(nameof(TransactionType), TransactionType, "Transaction Type", TransactionType),
            new(
                nameof(instruction.Notes),
                instruction.Notes,
                "Transaction Description",
                Truncate(instruction.Notes, MaxTransactionDescriptionLength)),
            new(
                nameof(instruction.PaymentDate),
                instruction.PaymentDate.ToString("O"),
                "Process Date",
                instruction.PaymentDate.ToString("yyMMdd", CultureInfo.InvariantCulture)),
            new(nameof(instruction.SourceCurrency), instruction.SourceCurrency, "Payment Currency", instruction.SourceCurrency),
            new(
                nameof(instruction.SourceAmount),
                instruction.SourceAmount.ToString(CultureInfo.InvariantCulture),
                "Payment Amount",
                FormatAmount(instruction.SourceAmount)),
            new(
                nameof(instruction.Amount),
                instruction.Amount.ToString(CultureInfo.InvariantCulture),
                "Debit Amount",
                FormatAmount(instruction.Amount)),
            new("DebitAccountBsb+DebitAccountNumber", debitAccountNumber, "Debit Account - Account Number", debitAccountNumber),
            new("", "", "Dealer Code", ""),
            new("", "", "Dealer Exchange Rate", ""),
            new(
                nameof(instruction.IntermediaryBankSwiftCode),
                instruction.IntermediaryBankSwiftCode,
                "Intermediary Bank - Bank Code",
                instruction.IntermediaryBankSwiftCode ?? ""),
            new(
                nameof(instruction.IntermediaryBankName),
                instruction.IntermediaryBankName,
                "Intermediary Bank - Name",
                instruction.IntermediaryBankName ?? ""),
            new("", "", "Intermediary Bank - City", ""),
            new(
                nameof(instruction.IntermediaryBankSwiftCode),
                instruction.IntermediaryBankSwiftCode,
                "Intermediary Institution - Country",
                intermediaryCountry),
            new(
                nameof(instruction.DestinationBankSwiftCode),
                instruction.DestinationBankSwiftCode,
                "Beneficiary Bank - Bank Code",
                instruction.DestinationBankSwiftCode),
            new(
                nameof(instruction.DestinationBankName),
                instruction.DestinationBankName,
                "Beneficiary Bank - Name",
                instruction.DestinationBankName),
            new("", "", "Beneficiary Bank - City", ""),
            new(
                nameof(instruction.DestinationBankSwiftCode),
                instruction.DestinationBankSwiftCode,
                "Beneficiary Bank - Country",
                beneficiaryCountry),
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
                SanitizeFreeText(instruction.BeneficiaryAddress)),
            new("", "", "Reserved for future use", ""),
            new("", "", "Reserved for future use", ""),
            new("", "", "Beneficiary - City", ""),
            new("", "", "Beneficiary - State", ""),
            new("", "", "Beneficiary - Postcode", ""),
            new(
                nameof(instruction.DestinationBankSwiftCode),
                instruction.DestinationBankSwiftCode,
                "Beneficiary - Country",
                beneficiaryCountry),
            new(
                nameof(instruction.PaymentReference),
                instruction.PaymentReference,
                "Beneficiary Payment Details",
                SanitizeFreeText(instruction.PaymentReference)),
        ];
    }

    // Field 7: last 4 digits of the static ImtSettings BSB (hyphens stripped) + the static account
    // number (spaces stripped). Batch-invariant - compute once per batch, not once per instruction.
    public static string DeriveDebitAccountNumber(ImtSettings settings) =>
        MappingUtilities.DeriveDebitAccountNumber(settings.DebitAccountBsb, settings.DebitAccountNumber);

    // Fields 20/27: replace disallowed characters (anything other than letters/digits/space/hyphen/
    // apostrophe, per §1.2 rule 7) with a space, then collapse repeated spaces - never drop characters
    // silently by deleting them outright, so a comma-separated address doesn't fuse two words together.
    public static string SanitizeFreeText(string value) =>
        RepeatedSpacesRegex().Replace(DisallowedFreeTextCharsRegex().Replace(value, " "), " ").Trim();

    private static string FormatAmount(decimal amount) =>
        amount > 0 ? amount.ToString(CultureInfo.InvariantCulture) : "";
}
