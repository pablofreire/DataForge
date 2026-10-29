using System.Buffers.Text;
using System.Globalization;
using System.Text;

namespace DataForge.Modules.Validation.Application.UseCases.Issues.Shared;

/// <summary>
/// Opaque keyset cursor for the issue list: the (row number, rule) of the last issue returned.
/// Encoded as base64url so clients treat it as a token rather than as identifiers.
/// </summary>
public readonly record struct IssueCursor(long RowNumber, long RuleId)
{
    public static readonly IssueCursor Start = new(-1, 0);

    public string Encode() =>
        Base64Url.EncodeToString(Encoding.ASCII.GetBytes(string.Create(CultureInfo.InvariantCulture, $"{RowNumber}:{RuleId}")));

    public static bool TryDecode(string? token, out IssueCursor cursor)
    {
        cursor = Start;
        if (string.IsNullOrEmpty(token))
        {
            return true;
        }

        try
        {
            var parts = Encoding.ASCII.GetString(Base64Url.DecodeFromChars(token)).Split(':');
            if (parts.Length == 2
                && long.TryParse(parts[0], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var rowNumber)
                && long.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var ruleId))
            {
                cursor = new IssueCursor(rowNumber, ruleId);
                return true;
            }
        }
        catch (FormatException)
        {
        }

        return false;
    }
}
