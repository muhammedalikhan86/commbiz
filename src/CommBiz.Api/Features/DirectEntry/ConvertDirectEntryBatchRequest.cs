namespace CommBiz.Api.Features.DirectEntry;

// One Direct Entry Detail Record's worth of input, matching the real upstream payload shape
// (verified against two real sample instructions). Static/organisation-level fields (LodgementReference,
// WithholdingTaxAmount, TransactionCode, Indicator, Trace BSB/Account) no longer come from the
// request — see DirectEntrySettings. DestinationBank* fields feed the CBA "BSB Number"/"Account
// Number to be Credited/Debited"/"Title of Account..." fields (see DirectEntryDetailRecordMapper);
// the organisation's own Trace settings feed the "Trace BSB"/"Trace Account"/"Name of Remitter"
// fields instead.
public record PaymentInstructionRequest(
    string PaymentTypeCode,
    string DestinationBankBsb,
    string DestinationBankAccountNo,
    string DestinationBankAccountName,
    DateTime PaymentDate,
    decimal Amount);
