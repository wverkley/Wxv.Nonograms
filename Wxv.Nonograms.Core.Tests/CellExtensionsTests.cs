using Wxv.Core;
using Xunit;
using Xunit.Abstractions;

namespace Wxv.Nonograms.Core.Tests;

public class CellExtensionsTests
{
    public ITestOutputHelper Logger { get; }

    public CellExtensionsTests(ITestOutputHelper logger)
    {
        Logger = logger;
    }

    [Theory]
    [InlineData(0, 2, 1)]
    [InlineData(1, 2, 2)]
    [InlineData(1, 3, 1)]
    [InlineData(2, 3, 3)]
    [InlineData(3, 3, 6)]
    [InlineData(3, 5, 1)]
    [InlineData(4, 5, 5)]
    public void GetUnsetLengthCombinations(int unsetSum, int unsetLengthCount, int expectedCombinationCount)
    {
        var combinations = unsetSum.GetUnsetLengthCombinations(unsetLengthCount).ToArray();
        var actualCombinationCount = combinations.Count();
        Logger.WriteLine($"unsetSum: {unsetSum}, unsetLengthCount: {unsetLengthCount}, expectedCombinationCount: {expectedCombinationCount}, actualCombinationCount: {actualCombinationCount}");
        foreach (var combination in combinations)
            Logger.WriteLine(combination.JoinString(", "));
        
        Assert.Equal(expectedCombinationCount, actualCombinationCount);
    }

    [Theory]
    [InlineData(1, "1", 1)]
    [InlineData(3, "1", 3)]
    [InlineData(3, "1,1", 1)]
    [InlineData(4, "1,1,1", 0)]
    [InlineData(5, "1,1,1", 1)]
    [InlineData(6, "1,1,1", 4)]
    [InlineData(6, "2,1,1", 1)]
    [InlineData(6, "1,1,2", 1)]
    [InlineData(7, "1,1,2", 4)]
    [InlineData(8, "1,1", 21)]
    [InlineData(8, "1,1,1", 20)]
    public void GetValidCellCombinations(int cellsCount, string lengthsString, int expectedCount)
    {
        Logger.WriteLine($"cellsCount: {cellsCount}, lengthsString: {lengthsString}, expectedCount: {expectedCount}");

        var lengths = lengthsString
            .Split(",")
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(int.Parse)
            .ToArray();
        
        var validCellCombinations = cellsCount.GetValidCellCombinations(lengths).ToList();
        Logger.WriteLine($"validCellCombinations:");
        foreach (var validCellCombination in validCellCombinations)
            Logger.WriteLine($"  \"{validCellCombination.ToDisplayString()}\"");
        
        Assert.Equal(expectedCount, validCellCombinations.Count);
    }

    [Theory]
    [InlineData("@", "", false)]
    [InlineData(".@.", "", false)]
    [InlineData("@@", "1", false)]
    [InlineData("@.@@", "1,1", false)]
    [InlineData("@@.@", "1,2", false)]
    [InlineData("@.@@", "2,1", false)]
    [InlineData("@.@@", "1,2", true)]
    [InlineData("@ @@", "1,2", true)]
    [InlineData("@@.@", "2,1", true)]
    [InlineData("@@ @", "2,1", true)]
    [InlineData("@@@.@", "2,1", false)]
    [InlineData("@.@@@", "1,2", false)]
    [InlineData("@@ @ .......", "1,2,1", false)]
    [InlineData("....... @ @@", "1,2,1", false)]
    [InlineData("........@@.@..", "1,6", true)]
    [InlineData("..@ @ @..", "1,1,1", true)]
    [InlineData("..@ @ @..", "1,1", false)]
    [InlineData("..@ @@..", "1,1", false)]
    [InlineData(".@@ @.. @", "1,2,1", false)]
    [InlineData("..@ ... @", "1,2,1", true)]
    [InlineData("    .........", "2", true)]
    [InlineData("    @........", "2", true)]
    [InlineData("    @.@......", "2", false)]
    [InlineData("@...@....", "5", true)]
    public void Validate(string cellsString, string lengthsString, bool expectedIsValid)
    {
        var cells = cellsString.ParseCells();
        var lengths = lengthsString
            .Split(",")
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(int.Parse)
            .ToArray();
        Assert.Equal(expectedIsValid, cells.Validate(lengths, CancellationToken.None));
    }
    
    [Theory]
    [InlineData("@ @..", "1,1", true, "@..", "1")]
    [InlineData("..@ @", "1,1", true, "..@", "1")]
    [InlineData("@ @..@ @", "1,1,1,1", true, "@..@", "1,1")]
    [InlineData("@ @...@ @", "1,1,1,1,1", true, "@...@", "1,1,1")]
    [InlineData("...@ @", "1", false, "", "")]
    [InlineData("...@ @", "1,1", true, "...@", "1")]
    [InlineData("...@ @", "2,1", true, "...@", "2")]
    [InlineData(".@@ @", "2,1", true, ".@@", "2")]
    [InlineData("", "", false, " ", "")]
    [InlineData("...", "1,1", false, "", "")]
    [InlineData("@@.@", "1,2", false, "", "")]
    [InlineData("@.@@", "2,1", false, "", "")]
    public void TryRemoveSolved(string cellsString, string lengthsString, bool expectedResult, string expectedUnsolvedCellsString, string expectedUnsolvedLengthsString)
    {
        Logger.WriteLine($"cellsString            : \"{cellsString}\"");
        Logger.WriteLine($"lengthsString          : \"{lengthsString}\"");

        var cells = cellsString.ParseCells();
        var lengths = lengthsString
            .Split(",")
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(int.Parse)
            .ToArray();
        var expectedUnsolvedCells = expectedUnsolvedCellsString.ParseCells();
        var expectedUnsolvedLengths = expectedUnsolvedLengthsString
            .Split(",")
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(int.Parse)
            .ToArray();
        var result = cells.TryRemoveSolved(
            lengths, 
            out var unsolvedCells, 
            out var unsolvedLengths, 
            out var removedPrefix, 
            out var removedSuffix);
        Logger.WriteLine(string.Empty);
        Logger.WriteLine($"expectedResult : {expectedResult}");
        Logger.WriteLine($"result         : {result}");
        
        Assert.Equal(expectedResult, result);
        
        if (result)
        {
            Logger.WriteLine(string.Empty);
            Logger.WriteLine($"unsolvedCells          : \"{unsolvedCells!.ToDisplayString()}\"");
            Logger.WriteLine($"expectedUnsolvedCells  : \"{expectedUnsolvedCells.ToDisplayString()}\"");
            Logger.WriteLine(string.Empty);
            Logger.WriteLine($"unsolvedLengths        : \"{unsolvedLengths!.JoinString(",")}\"");
            Logger.WriteLine($"expectedUnsolvedLengths: \"{expectedUnsolvedLengths.JoinString(",")}\"");
            Logger.WriteLine(string.Empty);
            Logger.WriteLine($"removedPrefix          : \"{removedPrefix!.ToDisplayString()}\"");
            Logger.WriteLine($"removedSuffix          : \"{removedSuffix!.ToDisplayString()}\"");

            Assert.True(unsolvedCells!.SequenceEqual(expectedUnsolvedCells));
            Assert.True(unsolvedLengths!.SequenceEqual(expectedUnsolvedLengths));
            Assert.Equal(cells.Length, removedPrefix!.Length + unsolvedCells!.Length + removedSuffix!.Length);
            Assert.True(unsolvedCells.Validate(unsolvedLengths!, CancellationToken.None));
        }
        else
        {
            Assert.Null(unsolvedCells);
            Assert.Null(unsolvedLengths);
            Assert.Null(removedPrefix);
            Assert.Null(removedSuffix);
        }
        
    }
    
}