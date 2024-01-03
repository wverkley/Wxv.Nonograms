using System.Text.RegularExpressions;

namespace Wxv.Parser;

internal static class ParserConstants
{
    public static readonly Regex ValidName 
        = new(@"^[_a-zA-Z][_a-zA-Z0-9\-+]*$", RegexOptions.CultureInvariant | RegexOptions.Compiled);
        
}