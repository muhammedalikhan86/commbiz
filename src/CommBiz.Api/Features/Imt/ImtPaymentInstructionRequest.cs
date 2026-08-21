namespace CommBiz.Api.Features.Imt;

// F-017: full IMT request shape, confirmed against a real sample payload (docs/stash/CommBiz File
// Specification - International Money Transfers Priority Payments Non CBA Payment Requests (MT101)
// v9.md §1.4).
public record ImtPaymentInstructionRequest(
    string PaymentTypeCode,
    string DestinationBankAccountName,
    string DestinationBankAccountNo,
    DateTime PaymentDate,
    string SourceCurrency,
    decimal SourceAmount,
    decimal Amount,
    string PaymentReference,
    string Notes,
    string DestinationBankSwiftCode,
    string DestinationBankName,
    string BeneficiaryAddress,
    string? IntermediaryBankSwiftCode,
    string? IntermediaryBankName);
