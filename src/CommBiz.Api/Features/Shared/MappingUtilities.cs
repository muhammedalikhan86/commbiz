namespace CommBiz.Api.Features.Shared;

// Shared across DirectEntry and BPay mappers (F-021 follow-up) - previously byte-identical private
// copies in each mapper, extracted here so every slice rounds/pads the exact same way.
public static class MappingUtilities
{
    public static long AmountToCents(decimal amount) =>
        (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);

    // Truncates rather than overflows a fixed-width record if a value is longer than its field.
    public static string FixedWidth(string value, int width) =>
        value.Length >= width ? value[..width] : value.PadRight(width);

    // Truncates a CSV/free-text field to a max length - unlike FixedWidth, never pads shorter values.
    public static string Truncate(string value, int maxLength) =>
        value.Length > maxLength ? value[..maxLength] : value;
}
