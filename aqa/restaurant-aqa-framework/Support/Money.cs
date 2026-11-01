using System.Globalization;
using System.Text.RegularExpressions;

namespace Restaurant_Aqa_Framework.Support;

public static partial class Money
{
    /// <summary>"$1,234.50" → 1234.50.</summary>
    public static decimal Parse(string text) =>
        decimal.Parse(NotAmount().Replace(text, ""), NumberStyles.Number, CultureInfo.InvariantCulture);

    /// <summary>The first whole number in a text: "Quantity: × 2" → 2.</summary>
    public static int FirstNumber(string text) => int.Parse(Digits().Match(text).Value, CultureInfo.InvariantCulture);

    [GeneratedRegex(@"[^0-9.\-]")]
    private static partial Regex NotAmount();

    [GeneratedRegex(@"\d+")]
    private static partial Regex Digits();
}
