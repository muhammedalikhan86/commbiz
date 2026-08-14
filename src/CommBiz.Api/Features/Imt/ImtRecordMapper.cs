using System.Globalization;
using System.Text.RegularExpressions;

namespace CommBiz.Api.Features.Imt;

// IMT record mapping (F-017, docs/stash/CommBiz File Specification - International Money Transfers
// Priority Payments Non CBA Payment Requests (MT101) v9.md §1.2/§1.4): manual field concatenation
// only, per ADR-004 (no AutoMapper). CSV, comma-delimited, 27 fields, NOT fixed-width/padded.
// SanitizeFreeText/DeriveCountryFromSwift are shared with ImtValidator so sanitization/derivation is
// each computed with one rule, not duplicated between validate and map.
public static partial class ImtRecordMapper
{
    private const string TransactionType = "IMT"; // field 1 constant - never "TT" (the API's routing code)

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
            instruction.Notes, // 2: Transaction Description
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

    // Field 7: last 4 digits of the static ImtSettings BSB (hyphens stripped) + the static account
    // number (spaces stripped). Batch-invariant - compute once per batch, not once per instruction.
    public static string DeriveDebitAccountNumber(ImtSettings settings)
    {
        var bsbDigits = settings.DebitAccountBsb.Replace("-", "");
        var last4 = bsbDigits.Length > 4 ? bsbDigits[^4..] : bsbDigits;
        var accountDigits = settings.DebitAccountNumber.Replace(" ", "");
        return last4 + accountDigits;
    }

    // A SWIFT BIC's 5th-6th characters are the ISO country code (e.g. CHASUS33 -> US), per §1.2
    // rules 11/12 and confirmed in the spec's Appendix C.
    public static string DeriveCountryFromSwift(string? swiftCode) =>
        swiftCode is { Length: >= 6 } ? swiftCode.Substring(4, 2).ToUpperInvariant() : "";

    // Fields 20/27: replace disallowed characters (anything other than letters/digits/space/hyphen/
    // apostrophe, per §1.2 rule 7) with a space, then collapse repeated spaces - never drop characters
    // silently by deleting them outright, so a comma-separated address doesn't fuse two words together.
    public static string SanitizeFreeText(string value) =>
        RepeatedSpacesRegex().Replace(DisallowedFreeTextCharsRegex().Replace(value, " "), " ").Trim();

    private static string FormatAmount(decimal amount) =>
        amount > 0 ? amount.ToString(CultureInfo.InvariantCulture) : "";
}
