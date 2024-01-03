using Wxv.Core;

namespace Wxv.Nonograms.Core;

public class Turn
{
    public int Width => Cells.Width;
    public int Height => Cells.Height;
    public IReadonlyGrid<Cell> Cells { get; internal init; } = new Grid<Cell>(0, 0);
    public IReadOnlyCollection<IReadOnlyCollection<bool>> HorizontalLengthsCompleted { get; internal init; } = ArraySegment<IReadOnlyCollection<bool>>.Empty;
    public IReadOnlyCollection<IReadOnlyCollection<bool>> VerticalLengthsCompleted { get; internal init; } = ArraySegment<IReadOnlyCollection<bool>>.Empty;
    public IReadonlyGrid<bool> WrongCells { get; internal init; } = new Grid<bool>(0, 0);
    public IReadonlyGrid<bool> HintCells { get; internal init; } = new Grid<bool>(0, 0);
    public bool IsFinished { get; internal init; }
    
    private string ToLazyString()
       => "#Turn" + Environment.NewLine
       + "H:" + Environment.NewLine
       + HorizontalLengthsCompleted
           .Select(completed => "[" + completed.Select(c => c ? "X" : " ").JoinString() + "]")
           .JoinString(Environment.NewLine) 
       + Environment.NewLine
       + "V:" + Environment.NewLine
       + VerticalLengthsCompleted
           .Select(completed => "[" + completed.Select(c => c ? "X" : " ").JoinString() + "]")
           .JoinString(Environment.NewLine)
       + Environment.NewLine
       + "C:" + Environment.NewLine
       + Enumerable
           .Range(0, Height)
           .Select(rowIndex => "[" + Cells.GetRow(rowIndex).Select(c => CellExtensions.DisplayCharacters[(int) c]).JoinString() + "]")
           .JoinString(Environment.NewLine)
       + Environment.NewLine
       + "W:" + Environment.NewLine
       + Enumerable
           .Range(0, Height)
           .Select(rowIndex => "[" + WrongCells.GetRow(rowIndex).Select(c => c ? "X" : " ").JoinString() + "]")
           .JoinString(Environment.NewLine)
       + Environment.NewLine
       + "I:" + Environment.NewLine
       + Enumerable
           .Range(0, Height)
           .Select(rowIndex => "[" + HintCells.GetRow(rowIndex).Select(c => c ? "X" : " ").JoinString() + "]")
           .JoinString(Environment.NewLine)
       + Environment.NewLine
       + "F:" + (IsFinished ? "Y" : "N");
    
    private Lazy<string> LazyString { get; }
    
    public Turn()
    {
        LazyString = new Lazy<string>(ToLazyString);
    }

    public override string ToString() => LazyString.Value;
    public override bool Equals(object? obj) 
        => obj is Turn other && string.Equals(ToString(), other.ToString(), StringComparison.Ordinal);
    public override int GetHashCode() => ToString().GetHashCode();
    
    public static Turn Parse(string content) => new TurnParser().Parse(content);

    public Turn? ApplyChanges(Cell[] changes, Orientation orientation, int index)
    {
        var hasChanged = false;
        Turn newTurn;
        
        if (orientation == Orientation.Horizontal)
        {
            if (changes.Length != Width || index < 0 || index >= Height)
                throw new ArgumentOutOfRangeException();

            var newCells = new Grid<Cell>(
                Width,
                Height,
                (x, y) =>
                {
                    var newCell = y == index && changes[x] != Cell.Unknown ? changes[x] : Cells[x, y];
                    hasChanged = hasChanged || Cells[x, y] != newCell;
                    return newCell;
                });

            newTurn = new Turn
            {
                HorizontalLengthsCompleted = HorizontalLengthsCompleted.Clone(),
                VerticalLengthsCompleted = VerticalLengthsCompleted.Clone(),
                Cells = newCells,
                WrongCells = WrongCells.CloneIfNotChanged(Cells, newCells),
                HintCells = HintCells.CloneIfNotChanged(Cells, newCells),
                IsFinished = IsFinished
            };
        }
        else
        {
            if (changes.Length != Height || index < 0 || index >= Width)
                throw new ArgumentOutOfRangeException();
            var newCells = new Grid<Cell>(
                Width,
                Height,
                (x, y) =>
                {
                    var newCell = x == index && changes[y] != Cell.Unknown ? changes[y] : Cells[x, y];
                    hasChanged = hasChanged || Cells[x, y] != newCell;
                    return newCell;
                }); 

            newTurn = new Turn
            {
                HorizontalLengthsCompleted = HorizontalLengthsCompleted.Clone(),
                VerticalLengthsCompleted = VerticalLengthsCompleted.Clone(),
                Cells = newCells,
                WrongCells = WrongCells.CloneIfNotChanged(Cells, newCells),
                HintCells = HintCells.CloneIfNotChanged(Cells, newCells),
                IsFinished = IsFinished
            };
        }

        return hasChanged ? newTurn : null;
    }

    public Turn? ApplyChanges(int x, int y, int width, int height, Cell cell)
    {
        if (x < 0 || width <= 0 || x + width > Width 
            || y < 0 || height <= 0 || y + height > Height)
            throw new ArgumentOutOfRangeException();
        
        var hasChanged = false;
        var newCells = new Grid<Cell>(
            Width,
            Height,
            (x0, y0) =>
            {
                var newCell = x0 >= x && x0 < x + width && y0 >= y && y0 < y + height
                    ? cell
                    : Cells[x0, y0];
                hasChanged = hasChanged || Cells[x0, y0] != newCell;
                return newCell;
            });
        
        var newTurn = new Turn
        {
            HorizontalLengthsCompleted = HorizontalLengthsCompleted.Clone(),
            VerticalLengthsCompleted = VerticalLengthsCompleted.Clone(),
            Cells = newCells,
            WrongCells = WrongCells.CloneIfNotChanged(Cells, newCells),
            HintCells = HintCells.CloneIfNotChanged(Cells, newCells),
            IsFinished = IsFinished
        };
        
        return hasChanged ? newTurn : null;
    }

    public Turn ApplyHorizontalLengthToggle(int y, int index)
    {
        var newHorizontalLengthsCompleted = HorizontalLengthsCompleted
            .Select((completedSet, y0) =>
                completedSet
                    .Select((c, index0) => y0 == y && index0 == index ? !c : c)
                    .ToArray())
            .ToArray();
        
        return new Turn
        {
            HorizontalLengthsCompleted = newHorizontalLengthsCompleted,
            VerticalLengthsCompleted = VerticalLengthsCompleted.Clone(),
            Cells = Cells.Clone(),
            WrongCells = WrongCells.Clone(),
            HintCells = HintCells.Clone(),
            IsFinished = IsFinished
        };
    }

    public Turn ApplyVerticalLengthToggle(int x, int index)
    {
        var newVerticalLengthsCompleted = VerticalLengthsCompleted
            .Select((completedSet, x0) =>
                completedSet
                    .Select((c, index0) => x0 == x && index0 == index ? !c : c)
                    .ToArray())
            .ToArray();
        
        return new Turn
        {
            HorizontalLengthsCompleted = HorizontalLengthsCompleted.Clone(),
            VerticalLengthsCompleted = newVerticalLengthsCompleted,
            Cells = Cells.Clone(),
            WrongCells = WrongCells.Clone(),
            HintCells = HintCells.Clone(),
            IsFinished = IsFinished
        };
    }

    public Turn? ApplyCheck(Solution solution)
    {
        var wrongCells = new Grid<bool>(
            Width, 
            Height, 
            (x, y) => Cells[x, y] != Cell.Unknown && Cells[x, y] != solution.Cells[x, y]);

        var horizontalLengthsCompleted = HorizontalLengthsCompleted.Clone();
        var verticalLengthsCompleted = VerticalLengthsCompleted.Clone();
        var cells = Cells.Clone();
        var hintCells = HintCells.Clone();
        var isFinished = IsFinished;
        if (!wrongCells.Any(v => v))
        {
            // if there are existing hint cells, complete them
            if (hintCells.Any(v => v))
            {
                for (var y = 0; y < Height; y++)
                    for (var x = 0; x < Width; x++)
                        if (hintCells[x, y])
                            cells[x, y] = solution.Cells[x, y];
                hintCells = new Grid<bool>(Width, Height);
            }
            // else just complete what we have 
            else
            {
                ApplyCheckFullyComplete(solution, cells, horizontalLengthsCompleted, verticalLengthsCompleted);

                if (!isFinished)
                    isFinished = cells.All(c => c != Cell.Unknown);
            }
        }
        

        var result = new Turn
        {
            HorizontalLengthsCompleted = horizontalLengthsCompleted,
            VerticalLengthsCompleted = verticalLengthsCompleted,
            Cells = cells,
            WrongCells = wrongCells,
            HintCells = hintCells,
            IsFinished = isFinished
        };

        return Equals(result) ? null : result;
    }

    /// <summary>
    /// If the turn is not wrong, complete and mark columns as rows that are fully complete
    /// </summary>
    private void ApplyCheckFullyComplete(
        Solution solution, 
        Grid<Cell> turnCells,
        bool[][] horizontalLengthsCompleted,
        bool[][] verticalLengthsCompleted)
    {
        bool cellsChanged;
        do
        {
            cellsChanged = false;
            for (var rowIndex = 0; rowIndex < Height; rowIndex++)
            {
                var solutionRow = solution.Cells.GetRow(rowIndex);
                var turnRow = turnCells.GetRow(rowIndex);
                if (solutionRow.AreCellsComplete(turnRow))
                {
                    for (var x = 0; x < turnCells.Width; x++)
                        if (turnCells[x, rowIndex] == Cell.Unknown)
                        {
                            turnCells[x, rowIndex] = solutionRow.ElementAt(x);
                            cellsChanged = true;
                        }

                    for (var i = 0; i < horizontalLengthsCompleted[rowIndex].Length; i++)
                        horizontalLengthsCompleted[rowIndex][i] = true;
                }
                else
                    ApplyCheckCompletedLengths(turnRow, horizontalLengthsCompleted[rowIndex]);
            }

            for (var columnIndex = 0; columnIndex < Width; columnIndex++)
            {
                var solutionColumn = solution.Cells.GetColumn(columnIndex);
                var turnColumn = turnCells.GetColumn(columnIndex);
                if (solutionColumn.AreCellsComplete(turnColumn))
                {
                    for (var y = 0; y < turnCells.Height; y++)
                        if (turnCells[columnIndex, y] == Cell.Unknown)
                        {
                            turnCells[columnIndex, y] = solutionColumn.ElementAt(y);
                            cellsChanged = true;
                        }

                    for (var i = 0; i < verticalLengthsCompleted[columnIndex].Length; i++)
                        verticalLengthsCompleted[columnIndex][i] = true;
                }
                else
                    ApplyCheckCompletedLengths(turnColumn, verticalLengthsCompleted[columnIndex]);
            }
        } while (cellsChanged);
    }

    /// <summary>
    /// Mark completed lengths if the turn is not wrong and the cells have not already been fully completed 
    /// </summary>
    private void ApplyCheckCompletedLengths(IEnumerable<Cell> turnCells, bool[] lengthCompleted)
    {
        {
            var completedCellCount = turnCells
                .GetConsecutive()
                .TakeWhile(c => c.Value != Cell.Unknown)
                .SkipWhile(c => c.Value == Cell.Unset)
                .Reverse()
                .SkipWhile(c => c.Value == Cell.Set)
                .Reverse()
                .Count(c => c.Value == Cell.Set);
            for (var i = 0; i < completedCellCount && i < lengthCompleted.Length; i++)
                if (!lengthCompleted[i])
                    lengthCompleted[i] = true;
        }

        {
            var completedCellCount = turnCells
                .Reverse()
                .GetConsecutive()
                .TakeWhile(c => c.Value != Cell.Unknown)
                .SkipWhile(c => c.Value == Cell.Unset)
                .Reverse()
                .SkipWhile(c => c.Value == Cell.Set)
                .Reverse()
                .Count(c => c.Value == Cell.Set);
            for (var i = 0; i < completedCellCount && lengthCompleted.Length - 1 - i >= 0; i++)
                if (!lengthCompleted[lengthCompleted.Length - 1 - i])
                    lengthCompleted[lengthCompleted.Length - 1 - i] = true;
        }
        
    }

    public Turn? ApplyHint(Solution solution)
    {
        var wrongCells = new Grid<bool>(
            Width, 
            Height, 
            (x, y) => Cells[x, y] != Cell.Unknown && Cells[x, y] != solution.Cells[x, y]);
        // has wrong cells, do a check instead
        if (wrongCells.Any(v => v))
            return ApplyCheck(solution);

        // already has hint cells
        if (HintCells.Any(v => v))
            return null;

        var solverEngine = new SolverEngine(Solvers.Registry.Solvers, solution);
        var solverEngineResult = solverEngine.Solve(solution.ToPuzzle(), this, CancellationToken.None);
        if (solverEngineResult == null)
            return null;

        var hintCells = new Grid<bool>(
            Width,
            Height,
            (x, y) => 
                (
                    (solverEngineResult.Cells.Orientation == Orientation.Horizontal && y == solverEngineResult.Cells.Index)
                    || (solverEngineResult.Cells.Orientation == Orientation.Vertical && x == solverEngineResult.Cells.Index)
                )
                && solverEngineResult.NextTurn.Cells[x, y] != Cell.Unknown
                && Cells[x, y] == Cell.Unknown);

        var result = new Turn
        {
            HorizontalLengthsCompleted = HorizontalLengthsCompleted.Clone(),
            VerticalLengthsCompleted = VerticalLengthsCompleted.Clone(),
            Cells = Cells.Clone(),
            WrongCells = wrongCells,
            HintCells = hintCells,
            IsFinished = IsFinished
        };

        return Equals(result) ? null : result;
    }

    public bool IsComplete()
    {
        for (var y = 0; y < Height; y++)
            if (Cells.GetRow(y).Any(c => c == Cell.Unknown))
                return false;
        return true;
    }
}