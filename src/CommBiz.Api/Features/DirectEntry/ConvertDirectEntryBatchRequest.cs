namespace CommBiz.Api.Features.DirectEntry;

// One Direct Entry Detail Record's worth of input, matching the real upstream payload shape
// (verified against two real sample instructions). Static/organisation-level fields (Title,
// LodgementReference, TraceBsb, RemitterName, WithholdingTaxAmount, TransactionCode, Indicator)
// no longer come from the request — see DirectEntrySettings.
public record PaymentInstructionRequest(
    string PaymentTypeCode,
    string AccountNo,
    string PaymentSourceTypeCode,
    string SourceBankAccountName,
    string SourceBankAccountNo,
    string SourceBankBsb,
    DateTime PaymentDate,
    string SourceCurrency,
    decimal SourceAmount,
    decimal Amount,
    string CreateBy);
