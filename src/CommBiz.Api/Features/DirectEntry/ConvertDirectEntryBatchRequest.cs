namespace CommBiz.Api.Features.DirectEntry;

// One Direct Entry Detail Record's worth of input, matching the real upstream payload shape
// (verified against two real sample instructions). Static/organisation-level fields (LodgementReference,
// WithholdingTaxAmount, TransactionCode, Indicator) no longer come from the request — see
// DirectEntrySettings. DestinationBank* fields feed the CBA "Trace"/"Name of Remitter" fields
// (see DirectEntryDetailRecordMapper) - the organisation's own Trace settings now feed the
// "BSB Number"/"Account Number to be Credited/Debited"/"Title of Account..." fields instead.
public record PaymentInstructionRequest(
    string PaymentTypeCode,
    string AccountNo,
    string SourceBankAccountName,
    string SourceBankAccountNo,
    string SourceBankBsb,
    string DestinationBankBsb,
    string DestinationBankAccountNo,
    string DestinationBankAccountName,
    DateTime PaymentDate,
    string SourceCurrency,
    decimal SourceAmount,
    decimal Amount,
    string CreateBy);
