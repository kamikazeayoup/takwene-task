using System.Text.RegularExpressions;

namespace TakweneTask.Domain.Helpers;

public static partial class Isrc
{
    private static readonly Regex IsrcRegex = GeneratedIsrcRegex();

    public static string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        return input.Replace("-", string.Empty)
                    .Replace(" ", string.Empty)
                    .Trim()
                    .ToUpperInvariant();
    }

    public static bool IsValid(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var normalized = Normalize(input);
        return IsrcRegex.IsMatch(normalized);
    }

    [GeneratedRegex(@"^[A-Z]{2}[A-Z0-9]{3}\d{7}$")]
    private static partial Regex GeneratedIsrcRegex();
}
