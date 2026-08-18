namespace CommBiz.Api.Features.Fx;

// F-023: FX request shape, per the Task Packet (docs/architecture.md §3 "FX Conversion Slice", FR-010).
// PaymentSourceTypeCode/PaymentDate/Notes/RateTypeCode/ValueDateTypeCode/FeeTypeCode/FeeOtherTypeCode
// are carried by the upstream payload for other purposes - not used in this feature's mapping or
// validation (same treatment IMT/Priority Payments give their own unused fields).
public record FxPaymentInstructionRequest(
    string PaymentTypeCode,
    string PaymentSourceTypeCode,
    DateTime PaymentDate,
    decimal Amount,
    string Notes,
    string BuyCurrency,
    string SellCurrency,
    string RateTypeCode,
    string ValueDateTypeCode,
    string FeeTypeCode,
    string FeeOtherTypeCode,
    string AccountNo);
