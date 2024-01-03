using Wxv.Core;

namespace Wxv.Nonograms.Core.Solvers;

/// <summary>
/// Try joining two spans of set cells with a single unknown square between them.
/// If it exceeds the max length then we know it must be unset
/// </summary>
public class Division : ISolver
{
    public string Name => nameof(Division);
    public bool IsReversable => false;
    public int Difficulty => 2;
    public bool ReturnFirstFound => false;


    public bool TrySolve(Cell[] cells, int[] lengths, out Cell[]? changes, CancellationToken cancellationToken)
    {
        changes = null;
        if (lengths.Length < 2)
            return false;

        var concecutive = cells.GetConsecutive().ToArray();
        var maxLength = lengths.Max();

        var result = cells.ToArray();
        var found = false;
        for (var i = 1; i < concecutive.Length - 1; i++)
        {
            if (concecutive[i - 1].Value != Cell.Set 
             || concecutive[i].Value != Cell.Unknown
             || concecutive[i].Count != 1
             || concecutive[i + 1].Value != Cell.Set
             || concecutive[i - 1].Count + concecutive[i + 1].Count + 1 <= maxLength)
                continue;

            result[concecutive[i].Index] = Cell.Unset;
            found = true;
        }
        if (!found)
            return false;

        changes = result;
        return true;
    }

}