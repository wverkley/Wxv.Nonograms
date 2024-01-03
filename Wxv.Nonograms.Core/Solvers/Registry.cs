namespace Wxv.Nonograms.Core.Solvers;

public static class Registry
{
    public static IReadOnlyCollection<ISolver> Solvers { get; } = new ISolver[]
    {
        // 1
        new Simple(),
        new FirstCellUnset(),
        // 2
        new Division(),
        new OffSideKnown(),
        new RemoveCantFitUknowns(),
        new UnsetBounds(),
        new FillKnowns(),
        // 3
        new FirstSuperPosition(),
        new RemoveImpossibleUnknowns(),
        // 4
        new SuperPosition(),
        // 10
        new Elimination(),
    };
}