using static CommBiz.Api.Features.Shared.MappingUtilities;

namespace CommBiz.Api.Features.DirectEntry;

// Shared cents-total computation (F-014, PM-005): the trailer and self-balancing mappers must agree on
// the batch total exactly, so it's computed once here and consumed by both, rather than each mapper
// rounding independently (which can drift by a cent on fractional-cent inputs).
public static class DirectEntryAmountTotals
{
    public static long SumAmountInCents(IReadOnlyList<PaymentInstructionRequest> instructions) =>
        instructions.Sum(instruction => AmountToCents(instruction.Amount));
}
