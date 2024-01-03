using Wxv.Core;

namespace Wxv.Nonograms.Core.Solvers;

/// <summary>
/// Remove unknown cell spans bounded by unsets where the minimum span length cannot fit in 
/// </summary>
public class RemoveCantFitUknowns : ISolver
{
    public string Name => nameof(RemoveCantFitUknowns);
    public bool IsReversable => false;
    public int Difficulty => 2;
    public bool ReturnFirstFound => false;
    
    public bool TrySolve(Cell[] cells, int[] lengths, out Cell[]? changes, CancellationToken cancellationToken)
    {
        changes = null;

        if (!lengths.Any())
            return false; // will already be handles by "simple"
        
        var minSetLength = lengths.Min();
        var consecutiveUnknowns = cells
            .GetConsecutive()
            .Where(c => 
                c.Value == Cell.Unknown 
                && c.Count < minSetLength 
                && (((c.Previous?.Value ?? Cell.Unset) == Cell.Unset)
                  && ((c.Next?.Value ?? Cell.Unset) == Cell.Unset)))
            .ToArray();
        if (!consecutiveUnknowns.Any())
            return false;
        
        changes = cells.ToArray();
        foreach (var consecutiveUnknown in consecutiveUnknowns)
        {
            for (var i = 0; i < consecutiveUnknown.Count; i++)
                changes[consecutiveUnknown.Index + i] = Cell.Unset;
        }
        
        return true;
    }
}