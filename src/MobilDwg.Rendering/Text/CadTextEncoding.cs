using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MobilDwg.Rendering.Text;

public static class CadTextEncoding
{
    private static readonly char[] Cp1254Lookup = CreateCp1254Table();
    private static readonly Regex AutoCadUnicodeRegex = new(@"\\[Uu]\+([0-9a-fA-F]{4})", RegexOptions.Compiled);
    private static readonly Regex AutoCadSymbolRegex = new(@"%%([cdpouCDPOU%])", RegexOptions.Compiled);

    static CadTextEncoding()
    {
        try
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        catch
        {
            // Fallback to built-in lookup table if CodePagesEncodingProvider is unavailable.
        }
    }

    /// <summary>
    /// Decodes raw bytes into a string, automatically detecting valid UTF-8 or falling back to CP1254 (Turkish ANSI).
    /// </summary>
    public static string DecodeBytes(ReadOnlySpan<byte> bytes)
    {
        if (bytes.IsEmpty) return string.Empty;

        // Try UTF-8 first
        var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
        try
        {
            return utf8.GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            // If UTF-8 validation fails, decode via Windows-1254 lookup
            return DecodeCp1254(bytes);
        }
    }

    /// <summary>
    /// Decodes bytes specifically using Windows-1254 (CP1254 Turkish).
    /// </summary>
    public static string DecodeCp1254(ReadOnlySpan<byte> bytes)
    {
        if (bytes.IsEmpty) return string.Empty;

        var chars = new char[bytes.Length];
        for (var i = 0; i < bytes.Length; i++)
        {
            chars[i] = Cp1254Lookup[bytes[i]];
        }
        return new string(chars);
    }

    /// <summary>
    /// Decodes AutoCAD Unicode escapes (\U+XXXX) and special symbol codes (%%d, %%p, %%c, %%%).
    /// </summary>
    public static string DecodeAutoCadEscapes(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        // 1. Decode \U+XXXX (e.g. \U+00E7 -> ç, \U+011F -> ğ, \U+0131 -> ı)
        var result = AutoCadUnicodeRegex.Replace(input, match =>
        {
            var hex = match.Groups[1].Value;
            if (int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var codePoint))
            {
                return ((char)codePoint).ToString();
            }
            return match.Value;
        });

        // 2. Decode %% symbols:
        // %%d or %%D -> ° (U+00B0)
        // %%p or %%P -> ± (U+00B1)
        // %%c or %%C -> Ø (U+00D8)
        // %%%        -> %
        // %%o / %%u  -> stripped overscore / underscore toggles
        result = AutoCadSymbolRegex.Replace(result, match =>
        {
            var code = char.ToLowerInvariant(match.Groups[1].Value[0]);
            return code switch
            {
                'd' => "\u00B0", // Degree
                'p' => "\u00B1", // Plus-Minus
                'c' => "\u00D8", // Diameter (Latin Capital Letter O with Stroke)
                '%' => "%",
                'o' or 'u' => string.Empty, // Strip formatting toggles in plain text
                _ => match.Value,
            };
        });

        return result;
    }

    private static char[] CreateCp1254Table()
    {
        var table = new char[256];
        // 0x00 - 0x7F: ASCII
        for (var i = 0; i < 128; i++)
        {
            table[i] = (char)i;
        }

        // 0x80 - 0x9F: Windows-1252 / 1254 control / printable extensions
        table[0x80] = '\u20AC'; // Euro
        table[0x81] = '\u0081';
        table[0x82] = '\u201A';
        table[0x83] = '\u0192';
        table[0x84] = '\u201E';
        table[0x85] = '\u2026';
        table[0x86] = '\u2020';
        table[0x87] = '\u2021';
        table[0x88] = '\u02C6';
        table[0x89] = '\u2030';
        table[0x8A] = '\u0160';
        table[0x8B] = '\u2039';
        table[0x8C] = '\u0152';
        table[0x8D] = '\u008D';
        table[0x8E] = '\u008E';
        table[0x8F] = '\u008F';
        table[0x90] = '\u0090';
        table[0x91] = '\u2018';
        table[0x92] = '\u2019';
        table[0x93] = '\u201C';
        table[0x94] = '\u201D';
        table[0x95] = '\u2022';
        table[0x96] = '\u2013';
        table[0x97] = '\u2014';
        table[0x98] = '\u02DC';
        table[0x99] = '\u2122';
        table[0x9A] = '\u0161';
        table[0x9B] = '\u203A';
        table[0x9C] = '\u0153';
        table[0x9D] = '\u009D';
        table[0x9E] = '\u009E';
        table[0x9F] = '\u0178';

        // 0xA0 - 0xFF: Latin-1 base with Turkish CP1254 specific substitutions
        for (var i = 0xA0; i <= 0xFF; i++)
        {
            table[i] = (char)i;
        }

        // Turkish CP1254 specific overrides:
        table[0xD0] = '\u011E'; // Ğ (Latin Capital Letter G with Breve)
        table[0xDD] = '\u0130'; // İ (Latin Capital Letter I with Dot Above)
        table[0xDE] = '\u015E'; // Ş (Latin Capital Letter S with Cedilla)
        table[0xF0] = '\u011F'; // ğ (Latin Small Letter G with Breve)
        table[0xFD] = '\u0131'; // ı (Latin Small Letter Dotless I)
        table[0xFE] = '\u015F'; // ş (Latin Small Letter S with Cedilla)

        return table;
    }
}
