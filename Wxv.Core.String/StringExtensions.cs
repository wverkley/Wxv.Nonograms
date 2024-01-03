using System.Net;

namespace Wxv.Core;

public static class StringExtensions
{
    public static StringRange Range(this string source) 
        => new(source);

    public static string ReflectToString(this object value) 
        => "{" + Environment.NewLine
            + string.Join(Environment.NewLine, value
                .GetType()
                .GetProperties()
                .Select(pi => $"  {pi.Name}={pi.GetValue(value)}"))
            + Environment.NewLine + "}";

    public static int IntParse(this string value, int defaultValue = int.MinValue) 
        => int.TryParse(value, out var result) ? result : defaultValue;

    public static bool BoolParse(this string value, bool defaultValue = false) 
        => bool.TryParse(value, out var result) ? result : defaultValue;

    public static double DoubleParse(this string value, double defaultValue = double.MinValue) 
        => double.TryParse(value, out var result) ? result : defaultValue;

    public static string HtmlDecode(this string value) 
        => WebUtility.HtmlDecode(value);

    public static string CommonSubstring(
        this ICollection<string> values, 
        StringComparison sc = StringComparison.InvariantCulture)
    {
        if (!values.Any()) return string.Empty;
        if (values.Count == 1) return values.First();

        var first = values.First();
        var result = string.Empty;
        for (var count = 1; count <= first.Length; count++)
        {
            for (var i = 0; i <= (first.Length - count); i++)
            {
                var s = first.Substring(i, count);
                if (values.All(v => v.IndexOf(s, 0, sc) >= 0))
                {
                    result = s;
                    break;
                }
            }
        }

        return result;
    }
}