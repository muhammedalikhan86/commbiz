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

    // A SWIFT BIC's 5th-6th characters are the ISO country code (e.g. CHASUS33 -> US) - shared by IMT/FX (dedup).
    public static string DeriveCountryFromSwift(string? swiftCode) =>
        swiftCode is { Length: >= 6 } ? swiftCode.Substring(4, 2).ToUpperInvariant() : "";

    // Last 4 digits of the BSB (hyphens stripped) + account number (spaces stripped) - shared by IMT/PP debit accounts.
    public static string DeriveDebitAccountNumber(string debitAccountBsb, string debitAccountNumber)
    {
        var bsbDigits = debitAccountBsb.Replace("-", "");
        var last4 = bsbDigits.Length > 4 ? bsbDigits[^4..] : bsbDigits;
        var accountDigits = debitAccountNumber.Replace(" ", "");
        return last4 + accountDigits;
    }

    // Positive, at most maxAmount, at most 2 decimal places - shared by FX/IMT/PP amount validation.
    public static bool IsValidAmountFormat(decimal amount, decimal maxAmount) =>
        amount > 0 && amount <= maxAmount && Math.Round(amount, 2) == amount;
}
