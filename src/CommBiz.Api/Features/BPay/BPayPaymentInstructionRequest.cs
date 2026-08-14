namespace CommBiz.Api.Features.BPay;

// F-016: full BPAY Batch Payments request shape, confirmed by the product owner (docs/stash/BPay
// Payments - CommBiz File Specification.md). AccountNo and PaymentSourceTypeCode are Shaw and Partners'
// own internal reference fields - validated but never mapped into the BPay output file.
public record BPayPaymentInstructionRequest(
    string PaymentTypeCode,
    string AccountNo,
    string PaymentSourceTypeCode,
    DateTime PaymentDate,
    decimal Amount,
    string BPayBillerCode,
    string BPayReference);
