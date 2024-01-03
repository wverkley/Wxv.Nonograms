namespace Wxv.Parser;

public static class ParserExtensions
{
    public static ParserResult Parse(this ParserEngine parser, TextReader reader, bool removeAnonymous = true)
    {
        if (reader == null)
            throw new ArgumentNullException(nameof(reader));

        var content = reader.ReadToEnd();
        return parser.Parse(content, removeAnonymous);
    }

    public static ParserResult ParseFile(this ParserEngine parser, string path, bool removeAnonymous = true)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentNullException(nameof(path));

        using var reader = new StreamReader(path);
        
        return parser.Parse(reader, removeAnonymous);
    }

    public static ParserResult First(this ParserResult parserResult, string name)
        => parserResult.First(pr => pr.Name == name);

    public static ParserResult? FirstOrDefault(this ParserResult parserResult, string name)
        => parserResult.FirstOrDefault(pr => pr.Name == name);

    public static IEnumerable<ParserResult> Where(this ParserResult parserResult, string name)
        => parserResult.Where(pr => pr.Name == name);

    public static bool Any(this ParserResult parserResult, string name)
        => parserResult.Any(pr => pr.Name == name);
}