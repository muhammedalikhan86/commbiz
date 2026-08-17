namespace CommBiz.Api.Features.PriorityPayments;

// F-018: full Priority Payments request shape, confirmed against a real sample payload
// (docs/stash/CommBiz File Specification - International Money Transfers Priority Payments Non CBA
// Payment Requests (MT101) v9.md §1.5). PaymentSourceTypeCode/SourceBankAccountName/SourceBankAccountNo/
// SourceBankBsb/SourceCurrency/SourceAmount are carried by the upstream payload for other purposes -
// not used in this feature's mapping or validation (same treatment IMT gives its own unused fields).
public record PriorityPaymentInstructionRequest(
    string PaymentTypeCode,
    string PaymentSourceTypeCode,
    string SourceBankAccountName,
    string SourceBankAccountNo,
    string SourceBankBsb,
    string DestinationBankAccountName,
    string DestinationBankAccountNo,
    string DestinationBankBsb,
    DateTime PaymentDate,
    string SourceCurrency,
    decimal SourceAmount,
    decimal Amount,
    string Notes,
    string? BeneficiaryAddress);
