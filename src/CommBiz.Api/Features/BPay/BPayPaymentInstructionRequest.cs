namespace CommBiz.Api.Features.BPay;

// F-016: full BPAY Batch Payments request shape, confirmed by the product owner (docs/stash/BPay
// Payments - CommBiz File Specification.md).
public record BPayPaymentInstructionRequest(
    string PaymentTypeCode,
    DateTime PaymentDate,
    decimal Amount,
    string BPayBillerCode,
    string BPayReference);
