namespace Wxv.Parser.IO;

public static class IOExtensions
{
    public static string Write(this ParserEngine parser)
    {
        using var writer = new StringWriter();
        var pw = new ParserWriter();
        pw.Write(parser, writer);
        return writer.ToString();
    }

    public static void Write(this ParserEngine parser, Stream stream)
    {
        var writer = new StreamWriter(stream);
        var pw = new ParserWriter();
        pw.Write(parser, writer);
    }
        
    public static void Write(this ParserEngine parser, TextWriter writer)
    {
        var pw = new ParserWriter();
        pw.Write(parser, writer);
    }

}