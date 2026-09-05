using System.Text;
using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Scene;

namespace MobilDwg.Rendering.Text;

public sealed record MTextParseResult(
    string PlainText,
    IReadOnlyList<string> Lines,
    string? ExtractedFontFamily,
    bool WasTruncated);

public static class MTextParser
{
    public const int MaxInputLength = 65536;
    public const int MaxNestingDepth = 32;
    public const int MaxLines = 4096;

    /// <summary>
    /// Parses an AutoCAD MTEXT string with strict safety budgets against ReDoS and deep nesting.
    /// Extracts plain text lines and any inline font/formatting directives.
    /// </summary>
    public static MTextParseResult Parse(
        string? mtext,
        ICollection<SceneDiagnostic>? diagnostics = null,
        RenderEntityId? entityId = null)
    {
        if (string.IsNullOrEmpty(mtext))
        {
            return new MTextParseResult(string.Empty, [string.Empty], null, false);
        }

        var wasTruncated = false;
        var input = mtext;
        if (input.Length > MaxInputLength)
        {
            input = input.Substring(0, MaxInputLength);
            wasTruncated = true;
            diagnostics?.Add(new SceneDiagnostic(
                SceneDiagnosticKind.Unsupported,
                "MTEXT_LENGTH_EXCEEDED",
                $"MTEXT length {mtext.Length} exceeded safety budget {MaxInputLength}; truncated.",
                entityId));
        }

        var lines = new List<string>();
        var currentLine = new StringBuilder();
        string? extractedFont = null;
        var depth = 0;
        var i = 0;

        while (i < input.Length)
        {
            var c = input[i];

            if (c == '{')
            {
                if (depth < MaxNestingDepth)
                {
                    depth++;
                }
                else
                {
                    diagnostics?.Add(new SceneDiagnostic(
                        SceneDiagnosticKind.Unsupported,
                        "MTEXT_NESTING_EXCEEDED",
                        $"MTEXT nesting depth exceeded maximum {MaxNestingDepth}.",
                        entityId));
                }
                i++;
                continue;
            }

            if (c == '}')
            {
                if (depth > 0)
                {
                    depth--;
                }
                i++;
                continue;
            }

            if (c == '\\')
            {
                if (i + 1 >= input.Length)
                {
                    i++;
                    break;
                }

                var next = input[i + 1];

                // Escaped backslash, braces
                if (next == '\\' || next == '{' || next == '}')
                {
                    currentLine.Append(next);
                    i += 2;
                    continue;
                }

                // Line breaks: \P or \X
                if (next == 'P' || next == 'p' || next == 'X' || next == 'x')
                {
                    if (lines.Count < MaxLines)
                    {
                        lines.Add(CadTextEncoding.DecodeAutoCadEscapes(currentLine.ToString()));
                    }
                    currentLine.Clear();
                    i += 2;
                    continue;
                }

                // Non-breaking space: \~
                if (next == '~')
                {
                    currentLine.Append(' ');
                    i += 2;
                    continue;
                }

                // Formatting toggles: \L, \l, \O, \o, \K, \k
                if (next is 'L' or 'l' or 'O' or 'o' or 'K' or 'k')
                {
                    i += 2;
                    continue;
                }

                // Font override: \F...; or \f...;
                if (next is 'F' or 'f')
                {
                    var semi = input.IndexOf(';', i + 2);
                    if (semi >= 0 && semi - i < 128)
                    {
                        var fontSpec = input.Substring(i + 2, semi - (i + 2));
                        // fontSpec may be "simplex.shx" or "Arial|b0|i0|c0|p34"
                        var pipe = fontSpec.IndexOf('|');
                        extractedFont = pipe >= 0 ? fontSpec.Substring(0, pipe) : fontSpec;
                        i = semi + 1;
                        continue;
                    }
                }

                // Stacking fraction: \S...^...;
                if (next is 'S' or 's')
                {
                    var semi = input.IndexOf(';', i + 2);
                    if (semi >= 0 && semi - i < 128)
                    {
                        var stackContent = input.Substring(i + 2, semi - (i + 2));
                        var sep = stackContent.IndexOfAny(['^', '/', '#']);
                        if (sep >= 0)
                        {
                            var num = stackContent.Substring(0, sep).Trim();
                            var den = stackContent.Substring(sep + 1).Trim();
                            currentLine.Append(num).Append('/').Append(den);
                        }
                        else
                        {
                            currentLine.Append(stackContent);
                        }
                        i = semi + 1;
                        continue;
                    }
                }

                // Other parameterized formatting tags: \A...;, \C...;, \H...;, \W...;, \Q...;, \T...;
                if (next is 'A' or 'a' or 'C' or 'c' or 'H' or 'h' or 'W' or 'w' or 'Q' or 'q' or 'T' or 't')
                {
                    var semi = input.IndexOf(';', i + 2);
                    if (semi >= 0 && semi - i < 128)
                    {
                        i = semi + 1;
                        continue;
                    }
                }

                // Unicode escape: \U+XXXX
                if ((next is 'U' or 'u') && i + 6 < input.Length && input[i + 2] == '+')
                {
                    var hexPart = input.Substring(i + 3, 4);
                    if (int.TryParse(hexPart, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out var codePoint))
                    {
                        currentLine.Append((char)codePoint);
                        i += 7;
                        continue;
                    }
                }

                // Unrecognized escape: skip backslash and keep character
                i++;
                continue;
            }

            currentLine.Append(c);
            i++;
        }

        if (lines.Count < MaxLines)
        {
            lines.Add(CadTextEncoding.DecodeAutoCadEscapes(currentLine.ToString()));
        }

        var plainText = string.Join("\n", lines);
        return new MTextParseResult(plainText, lines, extractedFont, wasTruncated);
    }
}
