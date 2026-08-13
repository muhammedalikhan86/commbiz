namespace CommBiz.Api.Features.DirectEntry;

// Batch header metadata + instruction array (Direct Entry spec's Header Record fields).
// F-005 owns real field validation; F-004 owns payment-type routing/rejection.
public record ConvertDirectEntryBatchRequest(
    string FileName,
    string UserIdentificationNumber,
    string DescriptionOfEntries,
    DateOnly DateToBeProcessed,
    IReadOnlyList<PaymentInstructionRequest> Instructions);

// One Direct Entry Detail Record's worth of input (field names/positions per the Direct Entry spec).
public record PaymentInstructionRequest(
    string PaymentType,
    string Bsb,
    string AccountNumber,
    string Indicator,
    string TransactionCode,
    long AmountInCents,
    string AccountTitle,
    string LodgementReference,
    string TraceBsb,
    string TraceAccountNumber,
    string RemitterName,
    long WithholdingTaxAmountInCents);
