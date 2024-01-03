using Wxv.Core;

namespace Wxv.Nonograms.Core;

public class Puzzle
{
    public IReadOnlyCollection<string> Description { get; internal init; } = Array.Empty<string>();
    public IReadOnlyCollection<IReadOnlyCollection<int>> HorizontalLengths { get; internal init; } = ArraySegment<IReadOnlyCollection<int>>.Empty;
    public IReadOnlyCollection<IReadOnlyCollection<int>> VerticalLengths { get; internal init; } = ArraySegment<IReadOnlyCollection<int>>.Empty;
    
    public int Width => VerticalLengths.Count;
    public int HeaderWidth => HorizontalLengths.Max(hl => hl.Count);
    public int TotalWidth => HeaderWidth + Width;
    public int Height => HorizontalLengths.Count;
    public int HeaderHeight => VerticalLengths.Max(hl => hl.Count);
    public int TotalHeight => HeaderHeight + Height;

    public override string ToString()
        => "#Puzzle" + Environment.NewLine
        + (Description.Any()
           ? string.Join(Environment.NewLine, Description.Select(d => "#" + d)) + Environment.NewLine
           : string.Empty)
        + "H:" + Environment.NewLine
        + HorizontalLengths.Select(l => "[" + l.JoinString(" ") + "]").JoinString(Environment.NewLine) + Environment.NewLine
        + "V:" + Environment.NewLine
        + VerticalLengths.Select(l => "[" + l.JoinString(" ") + "]").JoinString(Environment.NewLine);
    
    public Turn ToTurn() 
        => new()
        {
            Cells = new Grid<Cell>(Width, Height, (_, _) => Cell.Unknown)
        };

    public static Puzzle Parse(string content) => new PuzzleParser().Parse(content);
}