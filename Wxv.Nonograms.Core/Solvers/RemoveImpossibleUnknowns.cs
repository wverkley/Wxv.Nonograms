using Wxv.Core;

namespace Wxv.Nonograms.Core.Solvers;

public class RemoveImpossibleUnknowns : ISolver
{
    public string Name => nameof(RemoveImpossibleUnknowns);
    public bool IsReversable => false;
    public int Difficulty => 3;
    public bool ReturnFirstFound => false;
    
    public bool TrySolve(Cell[] cells, int[] lengths, out Cell[]? changes, CancellationToken cancellationToken)
    {
        changes = null;

        if (!lengths.Any())
            return false; // will already be handles by "simple"

        var concequativeSetOrUknowns = cells
            .Select(c => c != Cell.Unset)
            .GetConsecutive()
            .ToArray();
        var setOrUknownCount = concequativeSetOrUknowns.Count(c => c.Value);
        var setCount = concequativeSetOrUknowns
            .Count(c => c.Value && cells.Skip(c.Index).Take(c.Count).Any(c => c == Cell.Set));
        if (setCount > lengths.Length)
            return false; // it's invalid
        if (setCount < lengths.Length)
            return false; // we don't have a definative set span count yet 
        if (setOrUknownCount <= setCount)
            return false; // we don't have any extra unknown spans we can remove

        // now we know there are some spans with only unknowns and that cannot possibly have an set cells in them
        changes = cells.ToArray();
        foreach (var c in concequativeSetOrUknowns.Where(c => c.Value))
        {
            if (!cells.Skip(c.Index).Take(c.Count).All(c => c == Cell.Unknown))
                continue;

            for (var i = 0; i < c.Count; i++)
                changes[c.Index + i] = Cell.Unset;
        }
        return true;
    }
}