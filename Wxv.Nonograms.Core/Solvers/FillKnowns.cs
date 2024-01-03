using Wxv.Core;

namespace Wxv.Nonograms.Core.Solvers;

public class FillKnowns : ISolver
{
    public string Name => nameof(FillKnowns);
    public bool IsReversable => false;
    public int Difficulty => 3;
    public bool ReturnFirstFound => false;
    
    public bool TrySolve(Cell[] cells, int[] lengths, out Cell[]? changes, CancellationToken cancellationToken)
    {
        changes = null;

        if (!lengths.Any())
            return false; // will already be handled by "simple"

        var sets = cells
            .Select(c => c != Cell.Unset)
            .GetConsecutive()
            .Where(c => c.Value)
            .ToArray();
        if (sets.Length > lengths.Length)
            return false; // it's invalid

        if (sets.Any(set => Enumerable.Range(0, set.Count).All(j => cells[set.Index + j] == Cell.Unknown)))
            return false; // one span is at least fully unknown, so there is ambiguity 

        // now we know there are some spans with only knowns and that cannot possibly have an set cells in them
        var result = cells.ToArray();
        var changed = false;
        for (var i = 0; i < sets.Length; i++)
        {
            var set = sets[i];

            if (sets.Length == lengths.Length)
            { } // set count and lengths count equal, so the set spans only contain solution 
            else if (i == 0 && lengths.First() == set.Count)
            { } // first and lengths equal 
            else if (i == 0 && lengths.First() == set.Count - 1)
            { } // first and lengths almost equal
            else if (i == sets.Length - 1 && lengths.Last() == set.Count)
            { } // last and lengths equal 
            else if (i == sets.Length - 1 && lengths.Last() == set.Count - 1)
            { } // last and lengths almost equal
            else
                continue;
            
            var length =
                i == 0 ? lengths.First()
                : i == sets.Length - 1 ? lengths.Last()
                : lengths[i];

            if (Enumerable.Range(0, set.Count).All(j => cells[set.Index + j] == Cell.Set))
                continue; // already all set

            if (set.Count < length)
                return false; // invalid

            if (set.Count == length)
            {
                // span is the correct length....just set all to set
                for (var j = 0; j < length; j++)
                    result[set.Index + j] = Cell.Set;
                changed = true;
                continue;
            }
            
            var leadingUnknownCount = cells.Skip(set.Index).Take(set.Count).TakeWhile(c => c == Cell.Unknown).Count();
            var trailingUnknownCount = cells.Skip(set.Index).Take(set.Count).Reverse().TakeWhile(c => c == Cell.Unknown).Count();
            var knownCount = set.Count - leadingUnknownCount - trailingUnknownCount; 
            if (knownCount > length)
                return false; // invalid

            if (knownCount == length)
            {
                // we are fully known, all trailing and leading unknowns can be unset
                for (var j = 0; j < leadingUnknownCount; j++)
                    result[set.Index + j] = Cell.Unset;
                for (var j = 0; j < length; j++)
                    result[set.Index + leadingUnknownCount + j] = Cell.Set;
                for (var j = 0; j < trailingUnknownCount; j++)
                    result[set.Index + leadingUnknownCount + length + j] = Cell.Unset;
                changed = true;
                continue;
            }

            if (leadingUnknownCount == 0 && trailingUnknownCount > 0)
            {
                // trailing set
                for (var j = 0; j < length; j++)
                    result[set.Index + j] = Cell.Set;
                changed = true;
                trailingUnknownCount = set.Count - length;
                knownCount = length;
            }
            
            if (knownCount == length && leadingUnknownCount == 0 
              && (trailingUnknownCount == 1 || lengths.Length == 1 || lengths.Length == sets.Length))
            {
                // trailing unset
                for (var j = 0; j < trailingUnknownCount; j++)
                    result[set.Index + length + j] = Cell.Unset;
                changed = true;
            }

            if (trailingUnknownCount == 0 && leadingUnknownCount > 0)
            {
                // leading set 
                for (var j = 0; j < length; j++)
                    result[set.Index + set.Count - j - 1] = Cell.Set;
                changed = true;
                leadingUnknownCount = set.Count - length;
                knownCount = length;
            }
            
            if (knownCount == length && trailingUnknownCount == 0 
              && (leadingUnknownCount == 1 || lengths.Length == 1 || lengths.Length == sets.Length))
            {
                // leading unset
                for (var j = 0; j < leadingUnknownCount; j++)
                    result[set.Index + j] = Cell.Unset;
                changed = true;
            }
            
        }

        if (changed)
            changes = result;
        
        return changed;
    }
}