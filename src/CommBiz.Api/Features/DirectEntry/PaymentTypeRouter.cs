namespace CommBiz.Api.Features.DirectEntry;

// Payment Type Router (F-004, FR-006, architecture.md §3/§8 A3): only "DirectEntry" is supported
// in this tranche; any other declared type rejects the whole batch, no partial conversion.
public static class PaymentTypeRouter
{
    private const string SupportedPaymentType = "DirectEntry";

    public static IReadOnlyList<PaymentInstructionError>? FindUnsupportedPaymentTypes(
        IReadOnlyList<PaymentInstructionRequest> instructions)
    {
        List<PaymentInstructionError>? errors = null;

        for (var index = 0; index < instructions.Count; index++)
        {
            var paymentType = instructions[index].PaymentType;
            if (!string.Equals(paymentType, SupportedPaymentType, StringComparison.OrdinalIgnoreCase))
            {
                errors ??= [];
                errors.Add(new PaymentInstructionError(index, $"Unsupported payment type '{paymentType}'."));
            }
        }

        return errors;
    }
}
