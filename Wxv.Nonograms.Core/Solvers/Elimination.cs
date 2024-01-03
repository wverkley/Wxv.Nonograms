namespace Wxv.Nonograms.Core.Solvers;

/// <summary>
/// If something is valid when it's set, but invalid when it's unset, then it must be set
/// If something is valid when it's unset, but invalid when it's set, then it must be unset
///
/// This is an computationally expensive solver, so use at last resort
/// </summary>
public class Elimination : ISolver
{
    public string Name => nameof(Elimination);
    public bool IsReversable => false;
    public int Difficulty => 10; // validate is computionally expensive, so it should be a measure of last resort
    public bool ReturnFirstFound => true;

    public bool TrySolve(Cell[] cells, int[] lengths, out Cell[]? changes, CancellationToken cancellationToken)
    {
        changes = null;
        if (!lengths.Any())
            return false;

        var changed = false;
        bool found;
        var result = cells.ToArray();
        do
        {
            found = false;
            for (var i = 0; i < result.Length; i++)
            {
                if (result[i] != Cell.Unknown)
                    continue;

                var setChange = result.Select((c, ci) => i == ci ? Cell.Set : c).ToArray();
                var setChangeIsValid = setChange.Validate(lengths, cancellationToken);
                var unsetChange = result.Select((c, ci) => i == ci ? Cell.Unset : c).ToArray();
                var unsetChangeIsValid = unsetChange.Validate(lengths, cancellationToken);

                if (setChangeIsValid == unsetChangeIsValid)
                    continue;

                result = setChangeIsValid ? setChange : unsetChange;
                found = true;
                changed = true;
            }
        } while (found);

        if (!changed)
            return false;

        changes = result;
        return true;
    }

}