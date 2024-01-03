using Wxv.Core;
using Wxv.Nonograms.Core.Solvers;
using Xunit;
using Xunit.Abstractions;

namespace Wxv.Nonograms.Core.Tests;

public class SolverTests
{
    public ITestOutputHelper Logger { get; }

    public SolverTests(ITestOutputHelper logger)
    {
        Logger = logger;
    }
    
    private void Test(
        ISolver solver, 
        string cellsString, 
        string lengthsString, 
        bool expectedHasChanged, 
        string? expectedChanges, 
        bool tryRemoveSolved = false)
    {
        var cells = cellsString.ParseCells();
        var lengths = lengthsString
            .Split(",")
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(int.Parse)
            .ToArray();
        Logger.WriteLine($"{solver.Name}");
        Logger.WriteLine($" cells:      \"{cells.ToDisplayString()}\"");
        Logger.WriteLine($" lengths:    {lengths.JoinString(",")}");
        
        if (tryRemoveSolved && cells.TryRemoveSolved(lengths, out var changedCells, out var changedLengths,
                out var changedRemovedPrefix, out var changedRemovedSuffix))
        {
            cells = changedCells!;
            lengths = changedLengths!;
            Logger.WriteLine(string.Empty);
            Logger.WriteLine("after TryRemoveSolved:");
            Logger.WriteLine($" prefix:     \"{changedRemovedPrefix!.ToDisplayString()}\"");
            Logger.WriteLine($" cells:      \"{cells.ToDisplayString()}\"");
            Logger.WriteLine($" suffix:     \"{changedRemovedSuffix!.ToDisplayString()}\"");
            Logger.WriteLine($" lengths:    {lengths.JoinString(",")}");
        }
        
        var hasChanged = solver.TrySolve(cells, lengths, out var changes, CancellationToken.None);
        Logger.WriteLine(string.Empty);
        Logger.WriteLine("after TrySolve:");
        Logger.WriteLine($" hasChanged: {hasChanged}");
        Logger.WriteLine($" cells:      \"{cells.ToDisplayString()}\"");
        Logger.WriteLine($" changes:    \"{changes?.ToDisplayString()}\"");
        
        Assert.Equal(expectedHasChanged, hasChanged);
        Assert.Equal(expectedHasChanged, changes != null);
        if (expectedHasChanged)
        {
            Assert.Equal(cells.Length, changes!.Length);
            Assert.Equal(expectedChanges, changes.ToDisplayString());
            
            Assert.True(cells.AreChangesValid(changes));
        }
        else
            Assert.Null(changes);
    }

    [Theory]
    [InlineData(".....", "5", true, "@@@@@")]
    [InlineData(".....", "2,2", true, "@@ @@")]
    [InlineData(".....", "1,3", true, "@ @@@")]
    [InlineData("@...@", "1,1", true, "@   @")]
    [InlineData(".....", "", true, "     ")]
    [InlineData(". . .", "1,1,1", true, "@ @ @")]
    [InlineData(".....", "1", false, null)]
    public void Simple(
        string cellsString,
        string lengthsString,
        bool expectedSolve,
        string? expectedSolution)
        => Test(new Simple(), cellsString, lengthsString, expectedSolve, expectedSolution);
    
    [Theory]
    [InlineData(".....", "3", true, "..@..")]
    [InlineData("  .....", "3", true, "  ..@..")]
    [InlineData(".....  ", "3", true, "..@..  ")]
    [InlineData("  .....  ", "3", true, "  ..@..  ")]
    [InlineData(".....", "4", true, ".@@@.")]
    [InlineData("..........", "4,4", true, ".@@@..@@@.")]
    [InlineData("  ..........  ", "4,4", true, "  .@@@..@@@.  ")]
    [InlineData(".....", "1,2", true, "...@.")]
    [InlineData(".....", "1", false, null)]
    [InlineData(".....", "1,1", false, null)]
    public void SuperPosition(
        string cellsString,
        string lengthsString,
        bool expectedSolve,
        string? expectedSolution)
        => Test(new SuperPosition(), cellsString, lengthsString, expectedSolve, expectedSolution);    

    [Theory]
    [InlineData("@....", "3", true, "@@@ .")]
    [InlineData("@....", "4", true, "@@@@ ")]
    [InlineData("@....", "2,2", true, "@@ ..")]
    [InlineData("@..@@", "2,2", true, "@@ @@")]
    [InlineData(".....", "1", false, null)]
    [InlineData(" @...", "2", false, null)]
    [InlineData(".....", "1,1", false, null)]
    public void OffSideKnown(
        string cellsString,
        string lengthsString,
        bool expectedSolve,
        string? expectedSolution)
        => Test(new OffSideKnown(), cellsString, lengthsString, expectedSolve, expectedSolution);
    
    
    [Theory]
    [InlineData("@.@.", "1,1", true, "@ @.")]
    [InlineData("@@.@.", "2,1", true, "@@ @.")]
    [InlineData(".....", "1", false, null)]
    public void Division(
        string cellsString,
        string lengthsString,
        bool expectedSolve,
        string? expectedSolution)
        => Test(new Division(), cellsString, lengthsString, expectedSolve, expectedSolution);    
    
    
    [Theory]
    [InlineData("@@.@.", "2,2", true, "@@ @@")]
    [InlineData("@@ @.", "2,2", true, "@@ @@")]
    [InlineData("@@@@.", "4", true, "@@@@ ")]
    [InlineData("@@...", "2,1", true, "@@ ..")]
    [InlineData("........@@....", "1,6", false, null)]
    [InlineData(".....", "1", false, null)]
    [InlineData("    .........", "2", false, null)]
    [InlineData("    ...@.@...", "3", true, "       @@@   ")]
    [InlineData("    ....@....", "5", false, null)]

    public void Elimination(
        string cellsString,
        string lengthsString,
        bool expectedSolve,
        string? expectedSolution)
        => Test(new Elimination(), cellsString, lengthsString, expectedSolve, expectedSolution);    
    
    
    [Theory]
    [InlineData(".. .", "2", true, "..  ")]
    [InlineData(". .. ", "2", true, "  .. ")]
    [InlineData(" . .. ", "2", true, "   .. ")]
    [InlineData(".....", "1", false, null)]
    [InlineData("....@....", "7", false, null)]
    public void RemoveCantFitUnknowns(
        string cellsString,
        string lengthsString,
        bool expectedSolve,
        string? expectedSolution)
        => Test(new RemoveCantFitUknowns(), cellsString, lengthsString, expectedSolve, expectedSolution);    

    [Theory]
    [InlineData("..@@..", "1,2,1", true, ". @@ .")]
    [InlineData("...@@.@@...", "1,2,2,1", true, ".. @@ @@ ..")]
    [InlineData("...@..@@...", "1,2,2,1", true, "...@. @@ ..")]
    [InlineData("...@@..@...", "1,2,2,1", true, ".. @@ .@...")]
    [InlineData(".. @@ @@ ..", "1,2,2,1", false, null)]
    [InlineData("@...@", "2,2", false, null)]
    public void UnsetBounds(
        string cellsString,
        string lengthsString,
        bool expectedSolve,
        string? expectedSolution)
        => Test(new UnsetBounds(), cellsString, lengthsString, expectedSolve, expectedSolution);    
    
    [Theory]
    [InlineData("..@@", "2", true, " .@@")]
    [InlineData(".. ", "2", false, null)]
    [InlineData("@. ", "2", false, null)]
    [InlineData(" ..", "2", false, null)]
    [InlineData("....@@@@.", "1,5", false, null)]
    public void FirstCellUnset(
        string cellsString,
        string lengthsString,
        bool expectedSolve,
        string? expectedSolution)
        => Test(new FirstCellUnset(), cellsString, lengthsString, expectedSolve, expectedSolution);    

    
    [Theory]
    [InlineData(".@..", "3", true, ".@@.")]
    [InlineData(".@..@@", "3,2", true, ".@@.@@")]
    [InlineData(".@. @@", "3,2", true, "@@@ @@")]
    [InlineData(".@.......@@", "3,2", true, ".@@......@@")]
    [InlineData(".@.@@", "2,2", false, null)]
    [InlineData("@@..", "3", false, null)]
    [InlineData("..@..", "3", false, null)]
    [InlineData("..@.", "3", true, ".@@.")]
    [InlineData("....", "3", false, null)]
    [InlineData("....", "", false, null)]
    [InlineData("..@.@.", "3", false, null)]
    [InlineData(".@ ...", "2,3", true, "@@ ...")]
    public void FirstSuperPosition(
        string cellsString,
        string lengthsString,
        bool expectedSolve,
        string? expectedSolution)
        => Test(new FirstSuperPosition(), cellsString, lengthsString, expectedSolve, expectedSolution);    
    
    [Theory]
    [InlineData("@ .", "1", true, "@  ")]
    [InlineData("@. .", "1", true, "@.  ")]
    [InlineData("@. .@ .", "2,2", true, "@. .@  ")]
    [InlineData("@. . .@", "2,2", true, "@.   .@")]
    [InlineData(" @. . .@ ", "2,2", true, " @.   .@ " )]
    [InlineData(" @. . . .@ ", "2,2", true, " @.     .@ " )]
    [InlineData("@. .@ .", "1", false, null)]
    public void RemoveImpossibleUnknowns(
        string cellsString,
        string lengthsString,
        bool expectedSolve,
        string? expectedSolution)
        => Test(new RemoveImpossibleUnknowns(), cellsString, lengthsString, expectedSolve, expectedSolution);    

   
    [Theory]
    [InlineData(" @. ", "2", true, " @@ ")]
    [InlineData(" .@ ", "2", true, " @@ ")]
    [InlineData(" @. .", "2", false, null)]
    [InlineData(" .@ .", "2", false, null)]
    [InlineData(".@. .", "3", false, null)]
    [InlineData(" .@. .@. ", "3,3", true, " @@@ @@@ ")]
    [InlineData(". .@. .@. .", "3,3", false, null)]
    [InlineData(". .@. .@.", "3,3", false, null)]
    [InlineData(".@. .@. .", "3,3", false, null)]
    [InlineData("....@      @@@@@@   @@.", "2,6,2", true, "   @@      @@@@@@   @@ ")]
    [InlineData("......@ .", "2,1", false, null)]
    [InlineData("..@.", "1", true, "  @ ")]
    [InlineData("..@. @...", "1,4", true, "  @  @@@@")]
    [InlineData("@.@", "3", true, "@@@")]
    [InlineData(".@.@.", "3", true, " @@@ ")]
    [InlineData(" .@.@. ", "3", true, "  @@@  ")]
    [InlineData("......@.......@..", "9", true, "      @@@@@@@@@  ")]
    public void FillKnowns(
        string cellsString,
        string lengthsString,
        bool expectedSolve,
        string? expectedSolution)
        => Test(new FillKnowns(), cellsString, lengthsString, expectedSolve, expectedSolution);    

}