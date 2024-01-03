using Wxv.Core;

namespace Wxv.Nonograms.Core.Solvers;

/// <summary>
/// If the first span is unknown and it's length is equal to the first set length, and it's followed by a set span
/// then the first cell must be unset,
/// since the first set length couldn't fit in that unknown span without hitting the next set span and exceeding it's length
/// </summary>
public class FirstCellUnset : ISolver
{
    public string Name => nameof(FirstCellUnset);
    public bool IsReversable => true;
    public int Difficulty => 1;
    public bool ReturnFirstFound => false;


    public bool TrySolve(Cell[] cells, int[] lengths, out Cell[]? changes, CancellationToken cancellationToken)
    {
        changes = null;
        if (!lengths.Any())
            return false;

        var consecutive = cells.GetConsecutive().ToArray();
        var firstConsecutive = consecutive.First();
        if (firstConsecutive.Value != Cell.Unknown)
            return false;
        if (firstConsecutive.IsLast || firstConsecutive.Next!.Value != Cell.Set)
            return false;
        if (firstConsecutive.Count != lengths.First())
            return false;

        changes = cells.ToArray();
        changes[0] = Cell.Unset;
        return true;
    }

}