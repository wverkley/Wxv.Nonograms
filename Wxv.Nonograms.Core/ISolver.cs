namespace Wxv.Nonograms.Core;

public interface ISolver
{
    /// <summary>
    /// The solver name
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Should this be this be run on a reversed set of cells 
    /// </summary>
    bool IsReversable { get; }
    /// <summary>
    /// This difficulty of performing this solve
    /// </summary>
    int Difficulty { get; }
    /// <summary>
    /// Return the first solution only, don't go looking for more
    /// </summary>
    bool ReturnFirstFound { get; }
    bool TrySolve(Cell[] cells, int[] lengths, out Cell[]? changes, CancellationToken cancellationToken);
}