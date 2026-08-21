using System.Globalization;
using CommBiz.Api.Features.Shared;
using static CommBiz.Api.Features.Shared.MappingUtilities;

namespace CommBiz.Api.Features.Fx;

// FX record mapping (F-023, docs/stash/CommBiz IPFX Bulk Settlement Upload - File Specification
// v4.0 2.md "File Contents - Data Rows Format" / Samples 1-5): manual field concatenation only, per
// ADR-004 (no AutoMapper). CSV, comma-delimited, 27 fields. I SELL/I BUY Instruction (fields 7/12)
// always write the configured MAN/DOC constants - Shaw and Partners never routes an FX settlement to
// a real account (Samples 1/3/4/5's "Address Book Beneficiary" pattern) - but the beneficiary/
// intermediary bank fields (8/9/10/11/13/14/20) are still mapped through from the request whenever
// present, rather than left permanently blank, since the same shared payload IMT uses for these
// fields flows through here too. IDR/CNH/KRW conditional fields (23-27) remain deferred per
// architecture.md Open Question A6 - no source data for them exists in the payload at all.
public static class FxRecordMapper
{
    private const string TransactionType = "FX"; // field 1 constant

    // Field 8/10: Intermediary/Beneficiary Bank - Bank Code, up to 11AN.
    private const int MaxBankCodeLength = 11;

    // Field 13: Beneficiary - Account Name, up to 62AN.
    private const int MaxAccountNameLength = 62;

    // Field 14: Beneficiary - Address line 1, up to 40AN.
    private const int MaxAddressLength = 40;

    public static string Map(FxPaymentInstructionRequest instruction, FxSettings settings)
    {
        var intermediaryCountry = DeriveCountryFromSwift(instruction.IntermediaryBankSwiftCode);
        var beneficiaryCountry = DeriveCountryFromSwift(instruction.DestinationBankSwiftCode);

        return string.Join(
            ",",
            TransactionType, // 1: Transaction Type
            instruction.AccountNo, // 2: Transaction Description
            instruction.BuyCurrency, // 3: I BUY Currency
            "", // 4: I BUY Amount - Amount is always placed on the Sell side
            instruction.SellCurrency, // 5: I SELL Currency
            instruction.Amount.ToString(CultureInfo.InvariantCulture), // 6: I SELL Amount
            settings.SellInstruction, // 7: I SELL Instruction
            Truncate(instruction.IntermediaryBankSwiftCode ?? "", MaxBankCodeLength), // 8: Intermediary Bank - Bank Code
            intermediaryCountry, // 9: Intermediary Institution - Country
            Truncate(instruction.DestinationBankSwiftCode ?? "", MaxBankCodeLength), // 10: Beneficiary Bank - Bank Code
            beneficiaryCountry, // 11: Beneficiary Bank - Country
            settings.BuyInstruction, // 12: I BUY Instruction
            Truncate(instruction.DestinationBankAccountName ?? "", MaxAccountNameLength), // 13: Beneficiary - Account Name
            Truncate(instruction.BeneficiaryAddress ?? "", MaxAddressLength), // 14: Beneficiary - Address line 1
            "", // 15: Beneficiary - Address line 2 - always blank, rejected if populated
            "", // 16: Beneficiary - Address line 3 - always blank, rejected if populated
            "", // 17: Beneficiary - City/Suburb - no discrete field in the payload
            "", // 18: Beneficiary - State - no discrete field in the payload
            "", // 19: Beneficiary - Postcode - no discrete field in the payload
            beneficiaryCountry, // 20: Beneficiary - Country (same derivation as 11)
            settings.BuyPaymentDetails, // 21: I BUY Payment details
            settings.SellPaymentDetails, // 22: I SELL Payment details
            "", // 23: Purpose of Payment - IDR/CNH-specific, deferred (A6)
            "", // 24: CNAPS Code - CNH-specific, deferred (A6)
            "", // 25: Beneficiary Company Name - KRW-specific, deferred (A6)
            "", // 26: Beneficiary Contact Number - KRW-specific, deferred (A6)
            ""); // 27: Social Security Number (SSN) - KRW-specific, deferred (A6)
    }

    // One entry per FX CSV field position (27 total, same order as Map), including not-applicable
    // positions - a dropped position would break correspondence between Fields and the raw
    // comma-separated output line (the F-021 correction already applied to IMT/BPay/DE/PP).
    public static IReadOnlyList<FieldMapping> MapFields(FxPaymentInstructionRequest instruction, FxSettings settings)
    {
        var intermediaryCountry = DeriveCountryFromSwift(instruction.IntermediaryBankSwiftCode);
        var beneficiaryCountry = DeriveCountryFromSwift(instruction.DestinationBankSwiftCode);

        return
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
            new(
                nameof(instruction.IntermediaryBankSwiftCode),
                instruction.IntermediaryBankSwiftCode,
                "Intermediary Bank - Bank Code",
                Truncate(instruction.IntermediaryBankSwiftCode ?? "", MaxBankCodeLength)),
            new(nameof(instruction.IntermediaryBankSwiftCode), instruction.IntermediaryBankSwiftCode, "Intermediary Institution - Country", intermediaryCountry),
            new(
                nameof(instruction.DestinationBankSwiftCode),
                instruction.DestinationBankSwiftCode,
                "Beneficiary Bank - Bank Code",
                Truncate(instruction.DestinationBankSwiftCode ?? "", MaxBankCodeLength)),
            new(nameof(instruction.DestinationBankSwiftCode), instruction.DestinationBankSwiftCode, "Beneficiary Bank - Country", beneficiaryCountry),
            new(nameof(settings.BuyInstruction), settings.BuyInstruction, "I BUY Instruction", settings.BuyInstruction),
            new(
                nameof(instruction.DestinationBankAccountName),
                instruction.DestinationBankAccountName,
                "Beneficiary - Account Name",
                Truncate(instruction.DestinationBankAccountName ?? "", MaxAccountNameLength)),
            new(
                nameof(instruction.BeneficiaryAddress),
                instruction.BeneficiaryAddress,
                "Beneficiary - Address line 1",
                Truncate(instruction.BeneficiaryAddress ?? "", MaxAddressLength)),
            new("", "", "Beneficiary - Address line 2", ""),
            new("", "", "Beneficiary - Address line 3", ""),
            new("", "", "Beneficiary - City/Suburb", ""),
            new("", "", "Beneficiary - State", ""),
            new("", "", "Beneficiary - Postcode", ""),
            new(nameof(instruction.DestinationBankSwiftCode), instruction.DestinationBankSwiftCode, "Beneficiary - Country", beneficiaryCountry),
            new(nameof(settings.BuyPaymentDetails), settings.BuyPaymentDetails, "I BUY Payment details", settings.BuyPaymentDetails),
            new(nameof(settings.SellPaymentDetails), settings.SellPaymentDetails, "I SELL Payment details", settings.SellPaymentDetails),
            new("", "", "Purpose of Payment", ""),
            new("", "", "CNAPS Code", ""),
            new("", "", "Beneficiary Company Name", ""),
            new("", "", "Beneficiary Contact Number", ""),
            new("", "", "Social Security Number (SSN)", ""),
        ];
    }
}
