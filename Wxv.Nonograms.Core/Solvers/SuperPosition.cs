namespace Wxv.Nonograms.Core.Solvers;

/// <summary>
/// 
/// </summary>
public class SuperPosition : ISolver
{
    public string Name => nameof(SuperPosition);
    public bool IsReversable => false;
    public int Difficulty => 4;
    public bool ReturnFirstFound => false;
    
    public bool TrySolve(Cell[] cells, int[] lengths, out Cell[]? changes, CancellationToken cancellationToken)
    {
        changes = null;
        
        if (!lengths.Any())
            return false;

        var firstUnsetCells = cells.TakeWhile(c => c == Cell.Unset).ToArray();
        var lastUnsetCells = cells.Reverse().TakeWhile(c => c == Cell.Unset).Reverse().ToArray();
        var workingCells = cells
            .Skip(firstUnsetCells.Length)
            .Take(cells.Length - firstUnsetCells.Length - lastUnsetCells.Length)
            .ToArray();

        var known = lengths.Sum() + lengths.Length - 1;
        var unknown = workingCells.Length - known;
        if (known >= workingCells.Length || !lengths.Any(l => l > unknown))
            return false;

        var result = new List<Cell>();
        result.AddRange(firstUnsetCells);
        for (var i = 0; i < lengths.Length; i++)
        {
            if (i > 0)
                result.Add(Cell.Unknown);
            var length = lengths[i];
            if (length > unknown)
            {
                result.AddRange(Enumerable.Range(0, unknown).Select(_ => Cell.Unknown));
                result.AddRange(Enumerable.Range(0, length - unknown).Select(_ => Cell.Set));
            }
            else
                result.AddRange(Enumerable.Range(0, length).Select(_ => Cell.Unknown));
        };
        result.AddRange(Enumerable.Range(0, unknown).Select(_ => Cell.Unknown));
        result.AddRange(lastUnsetCells);
        changes = result.ToArray();
        return true;
    }
}