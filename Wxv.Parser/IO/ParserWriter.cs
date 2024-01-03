namespace Wxv.Parser.IO;

public class ParserWriter
{
    public void Write(ParserEngine parser, TextWriter writer)
    {
        if (parser == null)
            throw new ArgumentNullException(nameof(parser));
        if (writer == null)
            throw new ArgumentNullException(nameof(writer));

        writer.WriteLine(@"// MH.Parser");
        foreach (var symbol in parser)
            Write(symbol, writer);
    }

    private void Write(ParserSymbol symbol, TextWriter writer)
    {
        writer.WriteLine();
        writer.Write("{0}{1} := ", 
            symbol.IsRoot ? "@" : symbol.IsAnonymous ? "-" : "", 
            symbol.Name);

        var groupResults = symbol
            .Select(group => string.Join(" ", group.Select(WriteItem)))
            .ToArray();

        if (!groupResults.Any())
            writer.Write("");
        else if (groupResults.Length == 1)
            writer.Write(groupResults.First());
        else
            writer.Write(Environment.NewLine + "  " + string.Join(Environment.NewLine + "  | ", groupResults));

        writer.WriteLine(".");
    }

    private string WriteItem(ParserItem item)
    {
        return
            // Value
            (
                item is ParserRegex ? string.Format(@"""{0}""", ((ParserRegex)item).Match.Replace(@"""", @"""""")) 
                : item is ParserReference ? ((ParserReference)item).Name 
                : item is ParserEof ? "@eof" 
                : ""
            )

            +
            // Cardinality
            (
                item.MinOccurs == 1 && item.MaxOccurs == 1 ? ""
                : item.MinOccurs == 0 && item.MaxOccurs == 1 ? "?"
                : item.MinOccurs == 0 && item.MaxOccurs == int.MaxValue ? "*"
                : item.MinOccurs == 1 && item.MaxOccurs == int.MaxValue ? "+"
                : string.Format(@"[{0},{1}]", item.MinOccurs, item.MaxOccurs == int.MaxValue ? "*" : item.MinOccurs.ToString())
            )

            + 
            // No fallback
            (
                !item.CanFallback ? ">" 
                    : ""
            );
    }
}