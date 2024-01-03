namespace Wxv.Parser;

public readonly struct ParserPosition
{
    public int Index { get; private init; }
    public int Col { get; private init; }
    public int Row { get; private init; }

    public static readonly ParserPosition Start = new()
    {
        Index = 0,
        Col = 1,
        Row = 1
    };

    public ParserPosition Add(string content, int length)
    {
        if (content == null)
            throw new ArgumentNullException(nameof(content));
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        var resultIndex = Index;
        var resultCol = Col;
        var resultRow = Row;
        for (var i = 0; i < length && resultIndex < content.Length; i++)
        {
            if (content[resultIndex] == '\n')
            {
                resultCol = 1;
                resultRow++;
            }
            else
                resultCol++;
            resultIndex++;
        }
        return new ParserPosition
        {
            Index = resultIndex,
            Col = resultCol,
            Row = resultRow
        };
    }

    public override string ToString() => $"[{Index},{Col},{Row}]";
}