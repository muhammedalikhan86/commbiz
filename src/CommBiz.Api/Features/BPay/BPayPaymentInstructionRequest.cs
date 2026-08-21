namespace CommBiz.Api.Features.BPay;

// F-016: full BPAY Batch Payments request shape, confirmed by the product owner (docs/stash/BPay
// Payments - CommBiz File Specification.md). AccountNo is Shaw and Partners' own internal reference
// field - validated but never mapped into the BPay output file.
public record BPayPaymentInstructionRequest(
    string PaymentTypeCode,
    string AccountNo,
    DateTime PaymentDate,
    decimal Amount,
    string BPayBillerCode,
    string BPayReference);
