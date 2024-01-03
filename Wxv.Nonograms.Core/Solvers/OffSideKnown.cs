namespace Wxv.Nonograms.Core.Solvers;

/// <summary>
/// The cell on the edge is set
/// </summary>
public class OffSideKnown : ISolver
{
    public string Name => nameof(OffSideKnown);
    public bool IsReversable => true;
    public int Difficulty => 2;
    public bool ReturnFirstFound => false;
    
    public bool TrySolve(Cell[] cells, int[] lengths, out Cell[]? changes, CancellationToken cancellationToken)
    {
        changes = null;

        if (!lengths.Any())
            return false;
        var firstLength = lengths.First();
        if (firstLength >= cells.Length)
            return false; // will be handled by simple
        
        // first non-unset cell must be set
        if (cells.First() != Cell.Set)
            return false;

        changes = cells.ToArray();
        for (var i = 0; i < lengths.First(); i++)
            changes[i] = Cell.Set;
        changes[lengths.First()] = Cell.Unset; // cell after the length must be unset
        return true;
    }
}