namespace CommBiz.Api.Features.Imt;

// F-017: full IMT request shape, confirmed against a real sample payload (docs/stash/CommBiz File
// Specification - International Money Transfers Priority Payments Non CBA Payment Requests (MT101)
// v9.md §1.4). Several fields are nullable and carried by the upstream payload for other purposes
// (SourceBankAccountName/No/Bsb, DestinationBankTypeCode, Currency, the two IBAN fields,
// DestinationBankAddress, IntermediaryBankAddress) - not used in this feature's mapping.
public record ImtPaymentInstructionRequest(
    string PaymentTypeCode,
    string? SourceBankAccountName,
    string? SourceBankAccountNo,
    string? SourceBankBsb,
    string? DestinationBankTypeCode,
    string DestinationBankAccountName,
    string DestinationBankAccountNo,
    DateTime PaymentDate,
    string SourceCurrency,
    decimal SourceAmount,
    decimal Amount,
    string PaymentReference,
    string Notes,
    string? Currency,
    string? DestinationBankIBAN,
    string DestinationBankSwiftCode,
    string DestinationBankName,
    string? DestinationBankAddress,
    string BeneficiaryAddress,
    string? IntermediaryBankIBAN,
    string? IntermediaryBankSwiftCode,
    string? IntermediaryBankName,
    string? IntermediaryBankAddress);
