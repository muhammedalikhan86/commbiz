namespace CommBiz.Api.Features.PriorityPayments;

// F-018: full Priority Payments request shape, confirmed against a real sample payload
// (docs/stash/CommBiz File Specification - International Money Transfers Priority Payments Non CBA
// Payment Requests (MT101) v9.md §1.5).
public record PriorityPaymentInstructionRequest(
    string PaymentTypeCode,
    string DestinationBankAccountName,
    string DestinationBankAccountNo,
    string DestinationBankBsb,
    DateTime PaymentDate,
    decimal Amount,
    string Notes,
    string? BeneficiaryAddress);
