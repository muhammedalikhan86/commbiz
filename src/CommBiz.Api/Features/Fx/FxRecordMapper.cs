using System.Globalization;
using CommBiz.Api.Features.Shared;

namespace CommBiz.Api.Features.Fx;

// FX record mapping (F-023, docs/stash/CommBiz IPFX Bulk Settlement Upload - File Specification
// v4.0 2.md "File Contents - Data Rows Format" / Sample 2): manual field concatenation only, per
// ADR-004 (no AutoMapper). CSV, comma-delimited, 27 fields. Only the IPFX spec's non-CBA "Instruction"
// pattern is supported (Sample 2: I SELL Instruction = MAN, I BUY Instruction = DOC) - IDR/CNH/KRW
// conditional fields (23-27) are deferred per architecture.md Open Question A6.
public static class FxRecordMapper
{
    private const string TransactionType = "FX"; // field 1 constant

    public static string Map(FxPaymentInstructionRequest instruction, FxSettings settings) =>
        string.Join(
            ",",
            TransactionType, // 1: Transaction Type
            instruction.AccountNo, // 2: Transaction Description
            instruction.BuyCurrency, // 3: I BUY Currency
            "", // 4: I BUY Amount - Amount is always placed on the Sell side
            instruction.SellCurrency, // 5: I SELL Currency
            instruction.Amount.ToString(CultureInfo.InvariantCulture), // 6: I SELL Amount
            settings.SellInstruction, // 7: I SELL Instruction
            "", // 8: Intermediary Bank - Bank Code - not applicable
            "", // 9: Intermediary Institution - Country - not applicable
            "", // 10: Beneficiary Bank - Bank Code - not applicable
            "", // 11: Beneficiary Bank - Country - not applicable
            settings.BuyInstruction, // 12: I BUY Instruction
            "", // 13: Beneficiary - Account Name - not applicable
            "", // 14: Beneficiary - Address line 1 - not applicable
            "", // 15: Beneficiary - Address line 2 - not applicable
            "", // 16: Beneficiary - Address line 3 - not applicable
            "", // 17: Beneficiary - City/Suburb - not applicable
            "", // 18: Beneficiary - State - not applicable
            "", // 19: Beneficiary - Postcode - not applicable
            "", // 20: Beneficiary - Country - not applicable
            settings.BuyPaymentDetails, // 21: I BUY Payment details
            settings.SellPaymentDetails, // 22: I SELL Payment details
            "", // 23: Purpose of Payment - IDR/CNH-specific, deferred (A6)
            "", // 24: CNAPS Code - CNH-specific, deferred (A6)
            "", // 25: Beneficiary Company Name - KRW-specific, deferred (A6)
            "", // 26: Beneficiary Contact Number - KRW-specific, deferred (A6)
            ""); // 27: Social Security Number (SSN) - KRW-specific, deferred (A6)

    // One entry per FX CSV field position (27 total, same order as Map), including not-applicable
    // positions - a dropped position would break correspondence between Fields and the raw
    // comma-separated output line (the F-021 correction already applied to IMT/BPay/DE/PP).
    public static IReadOnlyList<FieldMapping> MapFields(FxPaymentInstructionRequest instruction, FxSettings settings) =>
        [
            new(nameof(TransactionType), TransactionType, "Transaction Type", TransactionType),
            new(nameof(instruction.AccountNo), instruction.AccountNo, "Transaction Description", instruction.AccountNo),
            new(nameof(instruction.BuyCurrency), instruction.BuyCurrency, "I BUY Currency", instruction.BuyCurrency),
            new("", "", "I BUY Amount", ""),
            new(nameof(instruction.SellCurrency), instruction.SellCurrency, "I SELL Currency", instruction.SellCurrency),
            new(
                nameof(instruction.Amount),
                instruction.Amount.ToString(CultureInfo.InvariantCulture),
                "I SELL Amount",
                instruction.Amount.ToString(CultureInfo.InvariantCulture)),
            new(nameof(settings.SellInstruction), settings.SellInstruction, "I SELL Instruction", settings.SellInstruction),
            new("", "", "Intermediary Bank - Bank Code", ""),
            new("", "", "Intermediary Institution - Country", ""),
            new("", "", "Beneficiary Bank - Bank Code", ""),
            new("", "", "Beneficiary Bank - Country", ""),
            new(nameof(settings.BuyInstruction), settings.BuyInstruction, "I BUY Instruction", settings.BuyInstruction),
            new("", "", "Beneficiary - Account Name", ""),
            new("", "", "Beneficiary - Address line 1", ""),
            new("", "", "Beneficiary - Address line 2", ""),
            new("", "", "Beneficiary - Address line 3", ""),
            new("", "", "Beneficiary - City/Suburb", ""),
            new("", "", "Beneficiary - State", ""),
            new("", "", "Beneficiary - Postcode", ""),
            new("", "", "Beneficiary - Country", ""),
            new(nameof(settings.BuyPaymentDetails), settings.BuyPaymentDetails, "I BUY Payment details", settings.BuyPaymentDetails),
            new(nameof(settings.SellPaymentDetails), settings.SellPaymentDetails, "I SELL Payment details", settings.SellPaymentDetails),
            new("", "", "Purpose of Payment", ""),
            new("", "", "CNAPS Code", ""),
            new("", "", "Beneficiary Company Name", ""),
            new("", "", "Beneficiary Contact Number", ""),
            new("", "", "Social Security Number (SSN)", ""),
        ];
}
