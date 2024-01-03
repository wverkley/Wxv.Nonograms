using Wxv.Core;

namespace Wxv.Nonograms.Core.Solvers;

/// <summary>
/// IF a span is of the max span lenght, then it's neighboring cells must be unset
/// </summary>
public class UnsetBounds : ISolver
{
    public string Name => nameof(UnsetBounds);
    public bool IsReversable => false;
    public int Difficulty => 2;
    public bool ReturnFirstFound => false;
    
    public bool TrySolve(Cell[] cells, int[] lengths, out Cell[]? changes, CancellationToken cancellationToken)
    {
        changes = null;
        if (!lengths.Any())
            return false;

        var maxSetLength = lengths.Max();
        var consecutiveSets = cells
            .GetConsecutive()
            .Where(c => 
                c.Value == Cell.Set 
                && c.Count == maxSetLength 
                && ((c.Previous?.Value ?? Cell.Unset) == Cell.Unknown
                 || (c.Next?.Value ?? Cell.Unset) == Cell.Unknown))
            .ToArray();
        if (!consecutiveSets.Any())
            return false;

        changes = cells.ToArray();
        foreach (var consecutiveSet in consecutiveSets)
        {
            if ((consecutiveSet.Previous?.Value ?? Cell.Unset) == Cell.Unknown)
                changes[consecutiveSet.Index - 1] = Cell.Unset;
            if ((consecutiveSet.Next?.Value ?? Cell.Unset) == Cell.Unknown)
                changes[consecutiveSet.Index + consecutiveSet.Count] = Cell.Unset;
        }

        return true;

    }
}