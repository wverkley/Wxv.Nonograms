using Wxv.Core;

namespace Wxv.Nonograms.Core.Solvers;

/// <summary>
/// Simple solves
/// </summary>
public class Simple : ISolver
{
    public string Name => nameof(Simple);
    public bool IsReversable => false;
    public int Difficulty => 1;
    public bool ReturnFirstFound => false;
    
    public bool TrySolve(Cell[] cells, int[] lengths, out Cell[]? changes, CancellationToken cancellationToken)
    {
        changes = null;
        
        // no lenghts, so cells must all be unset
        if (!lengths.Any())
        {
            changes = Enumerable
                .Range(0, cells.Length)
                .Select(_ => Cell.Unset)
                .ToArray();
            return true;
            
        }

        // length is 1 and equal to the cells
        if (lengths.Length == 1 && lengths.First() == cells.Length)
        {
            changes = Enumerable
                .Range(0, cells.Length)
                .Select(_ => Cell.Set)
                .ToArray();
            return true;
        }

        // length is 2 or more and sum with gaps is equal to the cells
        if (lengths.Length >= 2 && lengths.Sum() + lengths.Length - 1 == cells.Length)
        {
            var result = new List<Cell>();
            for (var i = 0; i < lengths.Length; i++)
            {
                if (i != 0)
                    result.Add(Cell.Unset);
                result.AddRange(Enumerable.Range(0, lengths[i]).Select(_ => Cell.Set));
            }
            changes = result.ToArray();
            return true;
        }
        
        // we've discovered all the set cells, so the rest must all be unset
        if (cells
            .GetConsecutive()
            .Where(c => c.Value == Cell.Set)
            .Select(c => c.Count)
            .ToArray().SequenceEqual(lengths))
        {
            changes = cells.Select(c => c == Cell.Set ? c : Cell.Unset).ToArray();
            return true;
        }

        // we've discovered all the unset cells, so the rest must all be set
        if (cells.Select(c => c == Cell.Unknown).Count() + cells.Select(c => c == Cell.Set).Count() == cells.Length)
        {
            changes = cells.Select(c => c == Cell.Unknown ? Cell.Set : c).ToArray();
            return true;
        }
        
        return false;
    }
}