using Wxv.Core;

namespace Wxv.Nonograms.Core.Solvers;

/// <summary>
/// Tries a super position over the first set 
/// </summary>
public class FirstSuperPosition : ISolver
{
    public string Name => nameof(FirstSuperPosition);
    public bool IsReversable => true;
    public int Difficulty => 3;
    public bool ReturnFirstFound => false;
    
    public bool TrySolve(Cell[] cells, int[] lengths, out Cell[]? changes, CancellationToken cancellationToken)
    {
        changes = null;
        
        if (!lengths.Any())
            return false;

        if (cells.First() != Cell.Unknown)
            return false;

        var firstLength = lengths.First();
        var consecutives = cells.GetConsecutive().ToArray();

        var nextConsecutive = consecutives.FirstOrDefault();
        
        // get the count of the leading uknowns
        var leadingUknownCount = nextConsecutive!.Count;
        nextConsecutive = nextConsecutive.Next;
        if (leadingUknownCount >= firstLength)
            return false; // too many leading uknowns, we cant determine anything
        
        if ((nextConsecutive?.Value ?? Cell.Unset) != Cell.Set)
            return false;
        var setCount = nextConsecutive!.Count;
        if (setCount > firstLength)
            return false; // it's too big compared to the first length
        nextConsecutive = nextConsecutive!.Next;
        
        var trailingUknownCount = 0;
        bool hasTrailingUnset;
        if ((nextConsecutive?.Value ?? Cell.Unset) == Cell.Unknown)
        {
            trailingUknownCount = nextConsecutive!.Count;
            hasTrailingUnset = (nextConsecutive.Next?.Value ?? Cell.Unset) == Cell.Unset;
        }
        else
            hasTrailingUnset = (nextConsecutive?.Value ?? Cell.Unset) == Cell.Unset;

        var result = cells.ToArray();
        var changed = false;
        
        // trailing sets
        if (leadingUknownCount + setCount < firstLength)
        {
            for (var i = 0; i < firstLength - leadingUknownCount - setCount && i < trailingUknownCount; i++)
            {
                if (result[leadingUknownCount + setCount + i] != Cell.Set)
                {
                    result[leadingUknownCount + setCount + i] = Cell.Set;
                    changed = true;
                }
            }
        }

        // leading sets
        if (setCount + trailingUknownCount < firstLength && hasTrailingUnset)
        {
            var count = firstLength - trailingUknownCount - setCount;
            for (var i = 0; i < count && leadingUknownCount - i - 1 >= 0; i++)
            {
                if (result[leadingUknownCount - i - 1] != Cell.Set)
                {
                    result[leadingUknownCount - i - 1] = Cell.Set;
                    changed = true;
                }
            }
        }
        if (!changed)
            return false;
        
        changes = result;
        return true;
    }
}