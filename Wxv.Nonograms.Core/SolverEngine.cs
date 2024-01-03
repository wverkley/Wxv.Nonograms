using Wxv.Core;
using Wxv.Nonograms.Core.Solvers;

namespace Wxv.Nonograms.Core;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor

public class SolverEngineCells
{
    public Cell[] OriginalCells { get; init; }
    public int[] OriginalLengths { get; init; }
    public int Index { get; init; }
    public Orientation Orientation { get; init; }
    public bool IsReverse { get; init; }
    public Cell[] WorkingCells { get; init; } = default!;
    public int[] WorkingLengths { get; init; } = default!;
    public Cell[] CompletedPrefix { get; init; } = default!;
    public Cell[] CompletedSuffix { get; init; } = default!;
    public int UnknownCellsCount { get; init; } 

    private SolverEngineCells()
    {
    }

    public SolverEngineCells(Cell[] originalCells, int[] originalLengths, int index, Orientation orientation)
    {
        OriginalCells = originalCells;
        OriginalLengths = originalLengths;
        Orientation = orientation;
        Index = index;
        IsReverse = false;
        if (originalCells.TryRemoveSolved(
                originalLengths,
                out var workingCells0,
                out var workingLengths0,
                out var completedPrefix0,
                out var completedSuffix0))
        {
            WorkingCells = workingCells0!;
            WorkingLengths = workingLengths0!;
            CompletedPrefix = completedPrefix0!;
            CompletedSuffix = completedSuffix0!;
        }
        else
        {
            WorkingCells = originalCells;
            WorkingLengths = originalLengths;
            CompletedPrefix = Array.Empty<Cell>();
            CompletedSuffix = Array.Empty<Cell>();
        }

        UnknownCellsCount = WorkingCells.Count(c => c == Cell.Unknown);
    }

    internal SolverEngineCells Reverse() 
        => new()
        {
            OriginalCells = OriginalCells.ToArray(),
            OriginalLengths = OriginalLengths.ToArray(),
            Orientation = Orientation,
            Index = Index,
            IsReverse = !IsReverse,
            WorkingCells = WorkingCells.Reverse().ToArray(),
            WorkingLengths = WorkingLengths.Reverse().ToArray(),
            CompletedPrefix = CompletedSuffix.Reverse().ToArray(),
            CompletedSuffix = CompletedPrefix.Reverse().ToArray(),
            UnknownCellsCount = UnknownCellsCount
        };

    public Cell[] GetFullChanges(Cell[] workingChanges)
    {
        if (workingChanges.Length != WorkingCells.Length)
            throw new ArgumentOutOfRangeException();
        var result = CompletedPrefix.Concat(workingChanges).Concat(CompletedSuffix);
        return !IsReverse
            ? result.ToArray()
            : result.Reverse().ToArray();
    }

    private string IsReverseAsString => IsReverse ? " (Reverse)" : string.Empty;
    private string OriginalLengthsAsString => IsReverse ? OriginalLengths.Reverse().JoinString(" ") : OriginalLengths.JoinString(" ");

    public override string ToString()
        => new[]
        {
            $"\"{OriginalCells.ToDisplayString()}\"",
            $"[{OriginalLengths.JoinString(" ")}]",
            $", I:{Index}",
            $", O:{Orientation}{IsReverseAsString}",
            $".  Working: \"{CompletedPrefix.ToDisplayString()}|{WorkingCells.ToDisplayString()}|{CompletedSuffix.ToDisplayString()}\"",
            $"[{OriginalLengthsAsString}]"
        }.JoinString();
}

public class SolverEngineResult
{
    public SolverEngineCells Cells { get; internal init; }
    public string SolverName { get; internal init; } = default!;
    public int Difficulty { get; internal init; }
    public IReadOnlyCollection<Cell> Changes { get; internal init; } = default!;
    public int ChangeCount { get; internal init; }
    public Turn NextTurn { get; internal init; } = default!;

    public override string ToString() =>
        $"{Cells}.{Environment.NewLine}Result: S:{SolverName}, D:{Difficulty} - [{(Cells.IsReverse ? Changes.Reverse() : Changes).ToDisplayString()}]";
}

public class SolverEngineInvalidException : Exception
{
    public SolverEngineCells Cells { get; }
    public string SolverName { get; }
    public IReadOnlyCollection<Cell> Changes { get; }

    internal SolverEngineInvalidException(SolverEngineCells cells, string solverName, IReadOnlyCollection<Cell> changes)
    {
        Cells = cells;
        SolverName = solverName;
        Changes = changes;
    }
    
    public override string ToString() =>
        $"{Cells}.{Environment.NewLine}Result: S:{SolverName} - [{(Cells.IsReverse ? Changes.Reverse() : Changes).ToDisplayString()}]";
}

public class SolverEngine
{
    public IReadOnlyCollection<ISolver> Solvers { get; }
    public Solution? Solution { get; }

    public SolverEngine(IReadOnlyCollection<ISolver>? solvers = null, Solution? solution = null)
    {
        Solvers = solvers ?? Registry.Solvers;
        Solution = solution;
    }

    public SolverEngineResult? Solve(Puzzle puzzle, Turn turn, CancellationToken cancellationToken)
    {
        var solverEngineCellsList = new List<SolverEngineCells>();

        for (var rowIndex = 0; rowIndex < turn.Height; rowIndex++)
        {
            var row = turn.Cells.GetRow(rowIndex).ToArray();
            var lengths = puzzle.HorizontalLengths.ElementAt(rowIndex).ToArray();
            if (row.IsSolved(lengths))
                continue;
            
            solverEngineCellsList.Add(new SolverEngineCells(row, lengths, rowIndex, Orientation.Horizontal));
        }

        for (var columnIndex = 0; columnIndex < turn.Width; columnIndex++)
        {
            var row = turn.Cells.GetColumn(columnIndex).ToArray();
            var lengths = puzzle.VerticalLengths.ElementAt(columnIndex).ToArray();
            if (row.IsSolved(lengths))
                continue;

            solverEngineCellsList.Add(new SolverEngineCells(row, lengths, columnIndex, Orientation.Vertical));
        }
        
        // sort by unknown count
        solverEngineCellsList = solverEngineCellsList.OrderBy(c => c.UnknownCellsCount).ToList();

        foreach (var solver in Solvers.OrderBy(solver => solver.Difficulty))
        {
            var solverEngineResult = Solve(turn, solver, solverEngineCellsList, cancellationToken);
            if (solverEngineResult != null)
                return solverEngineResult;
        }
        return null;
    }

    private SolverEngineResult? Solve(
        Turn turn, 
        ISolver solver, 
        List<SolverEngineCells> solverEngineCellsList, 
        CancellationToken cancellationToken)
    {
        var results = new List<SolverEngineResult>();

        foreach (var solverEngineCells in solverEngineCellsList)
        {
            results.AddRange(Solve(turn, solver, solverEngineCells, cancellationToken));
            if (solver.ReturnFirstFound && results.Any())
                return results.MinBy(r => (r.Difficulty, -r.ChangeCount));

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException();
        }

        return results.MinBy(r => (r.Difficulty, -r.ChangeCount));
    }

    public IEnumerable<SolverEngineResult> Solve(
        Turn turn, 
        ISolver solver, 
        SolverEngineCells solverEngineCells, 
        CancellationToken cancellationToken)
    {
        var solverEngineCellsReverse = solverEngineCells.Reverse();
        {
            var solverEngineResult = Solve(turn, solverEngineCells, solver, cancellationToken);
            if (solverEngineResult != null)
            {
                yield return solverEngineResult;
                if (solver.ReturnFirstFound)
                    yield break;
            }
        }
        if (solver.IsReversable)
        {
            var solverEngineResult = Solve(turn, solverEngineCellsReverse, solver, cancellationToken);
            if (solverEngineResult != null)
                yield return solverEngineResult;
        }
    }

    private SolverEngineResult? Solve(
        Turn turn,
        SolverEngineCells solverEngineCells,
        ISolver solver, 
        CancellationToken cancellationToken)
    {
        if (!solver.TrySolve(solverEngineCells.WorkingCells, solverEngineCells.WorkingLengths, out var workingChanges, cancellationToken))
            return null;

        var changeCount = solverEngineCells.WorkingCells.CountChanges(workingChanges!);
        if (changeCount <= 0)
            return null;

        var fullChanges = solverEngineCells.GetFullChanges(workingChanges!);
        if (fullChanges.Length != solverEngineCells.OriginalCells.Length)
            throw new SolverEngineInvalidException(solverEngineCells, solver.Name, fullChanges);
        if (Solution == null && !fullChanges.Validate(solverEngineCells.OriginalLengths, cancellationToken))
            throw new SolverEngineInvalidException(solverEngineCells, solver.Name, fullChanges);

        var nextTurn = turn.ApplyChanges(fullChanges, solverEngineCells.Orientation, solverEngineCells.Index)!;
        if (!Solution?.IsTurnCorrect(nextTurn) ?? false)
            throw new SolverEngineInvalidException(solverEngineCells, solver.Name, fullChanges);

        return new SolverEngineResult
        {
            Cells = solverEngineCells,
            SolverName = solver.Name,
            Difficulty = solver.Difficulty,
            Changes = fullChanges,
            ChangeCount = changeCount,
            NextTurn = nextTurn
        };
    }
}