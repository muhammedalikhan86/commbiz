namespace CommBiz.Api.Features.DirectEntry;

// Payment Type Router (F-004, FR-006, architecture.md §3/§8 A3): only "DE" is supported
// in this tranche; any other declared type rejects the whole batch, no partial conversion.
public static class PaymentTypeRouter
{
    private const string SupportedPaymentType = "DE";

    public static IReadOnlyList<PaymentInstructionError>? FindUnsupportedPaymentTypes(
        IReadOnlyList<PaymentInstructionRequest> instructions)
    {
        List<PaymentInstructionError>? errors = null;

        for (var index = 0; index < instructions.Count; index++)
        {
            var paymentTypeCode = instructions[index].PaymentTypeCode;
            if (!string.Equals(paymentTypeCode, SupportedPaymentType, StringComparison.OrdinalIgnoreCase))
            {
                errors ??= [];
                errors.Add(new PaymentInstructionError(index, $"Unsupported payment type '{paymentTypeCode}'."));
            }
        }

        return errors;
    }
}
