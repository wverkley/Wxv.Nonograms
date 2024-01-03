using Wxv.Core;

namespace Wxv.Nonograms.Core;

public class Solution
{
    public IReadOnlyCollection<string> Description { get; internal init; } = Array.Empty<string>();
    public int Width => Cells.Width;
    public int Height => Cells.Height;
    public IReadonlyGrid<Cell> Cells { get; internal init; } = new Grid<Cell>(0, 0);

    internal Solution()
    {
    }

    public Solution(Grid<Cell> cells, IReadOnlyCollection<string>? description = null)
    {
        if (cells.Width <= 0 || cells.Height <= 0)
            throw new ArgumentOutOfRangeException();
        
        Description = description ?? Array.Empty<string>();
        Cells = cells;
    }

    public override string ToString()
        => "#Solution"  + Environment.NewLine
        + (Description.Any() 
            ? string.Join(Environment.NewLine, Description.Select(d => "#" + d)) + Environment.NewLine 
            : string.Empty)
        + Enumerable
            .Range(0, Height)
            .Select(rowIndex => "[" + Cells.GetRow(rowIndex).Select(c => CellExtensions.DisplayCharacters[(int) c]).JoinString() + "]")
            .JoinString(Environment.NewLine);

    public Turn ToTurn()
    {
        var puzzle = ToPuzzle();
        return new Turn
        {
            HorizontalLengthsCompleted = puzzle
                .HorizontalLengths
                .Select(hl => Enumerable
                    .Range(0, hl.Count)
                    .Select(_ => false)
                    .ToArray())
                .ToArray(),
            VerticalLengthsCompleted = puzzle
                .VerticalLengths
                .Select(hl => Enumerable
                    .Range(0, hl.Count)
                    .Select(_ => false)
                    .ToArray())
                .ToArray(),
            Cells = new Grid<Cell>(Width, Height, (_, _) => Cell.Unknown),
            WrongCells = new Grid<bool>(Width, Height),
            HintCells = new Grid<bool>(Width, Height)
        };
    }

    public Turn ToSolvedTurn()
    {
        var puzzle = ToPuzzle();
        return new Turn
        {
            HorizontalLengthsCompleted = puzzle
                .HorizontalLengths
                .Select(hl => Enumerable
                    .Range(0, hl.Count)
                    .Select(_ => true)
                    .ToArray())
                .ToArray(),
            VerticalLengthsCompleted = puzzle
                .VerticalLengths
                .Select(hl => Enumerable
                    .Range(0, hl.Count)
                    .Select(_ => true)
                    .ToArray())
                .ToArray(),
            Cells = new Grid<Cell>(Width, Height, (x, y) => Cells[x, y] == Cell.Set ? Cell.Set : Cell.Unset),
            WrongCells = new Grid<bool>(Width, Height),
            HintCells = new Grid<bool>(Width, Height)
        };
    }

    public bool IsTurnCorrect(Turn turn)
    {
        if (turn.Width != Width && turn.Height != Height)
            throw new InvalidOperationException();
        
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
        {
            if (turn.Cells[x, y] == Cell.Unknown)
                continue;
            if (turn.Cells[x, y] != Cells[x, y])
                return false;
        }

        return true;
    }

    public Puzzle ToPuzzle()
    {
        return new Puzzle
        {
            Description = Description,
            HorizontalLengths = Cells
                .Rows
                .Select(row => row
                    .GetConsecutive()
                    .Where(c => c.Value == Cell.Set)
                    .Select(c => c.Count)
                    .ToArray())
                .ToArray(),
            VerticalLengths = Cells
                .Columns
                .Select(row => row
                    .GetConsecutive()
                    .Where(c => c.Value == Cell.Set)
                    .Select(c => c.Count)
                    .ToArray())
                .ToArray()
        };
    }

    public static Solution Parse(string content) => new SolutionParser().Parse(content);
}