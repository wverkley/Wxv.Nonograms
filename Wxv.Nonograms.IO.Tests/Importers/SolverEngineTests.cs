using Wxv.Core;
using Wxv.Nonograms.Core;
using Wxv.Nonograms.IO.Importers;
using Xunit.Abstractions;

namespace Wxv.Nonograms.IO.Tests.Importers;

public class SolverEngineTests
{
    public ITestOutputHelper Logger { get; }

    public SolverEngineTests(ITestOutputHelper logger)
    {
        Logger = logger;
    }

    [Theory]
    [InlineData("Image1.png", 15, 13)]
    [InlineData("Image2.png", 13, 15)]
    [InlineData("Image3.png", 20, 30)]
    [InlineData("Image4.png", 45, 35)]
    [InlineData("Image5.png", 5, 7)]
    [InlineData("Image6.png", 7, 11)]
    //[InlineData("Image7.png", 40, 45)]
    [InlineData("Image8.png", 20, 8)]
    [InlineData("Image9.png", 15, 15)]
    [InlineData("Image10.png", 20, 20)]
    public async Task SolveFromImage(string filename, int expectedWidth, int expectedHeight)
    {
        await using var stream = typeof(SolverEngineTests).Assembly.GetManifestResourceStream(typeof(Tests).Namespace + ".AnswerImages." + filename);
        var data = await stream!.ReadAllBytes();

        var answerImageImporter = new AnswerImageImporter();
        var result = await answerImageImporter.TryImportAsync(data!, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedWidth, result.Solution!.Width);
        Assert.Equal(expectedHeight, result.Solution!.Height);

        var solution = result.Solution!;
        Logger.WriteLine(Environment.NewLine + solution);

        var puzzle = solution.ToPuzzle();
        Logger.WriteLine(Environment.NewLine + puzzle);

        var turn = solution.ToTurn();
        
        Logger.WriteLine(Environment.NewLine + turn);

        var solverEngine = new SolverEngine(solution: solution);
        SolverEngineResult? solverEngineResult;
        try
        {
            while ((solverEngineResult = solverEngine.Solve(puzzle, turn, CancellationToken.None)) != null)
            {
                turn = solverEngineResult.NextTurn;
                Logger.WriteLine(Environment.NewLine + solverEngineResult);
            }

            Logger.WriteLine(Environment.NewLine + turn);

            Assert.True(turn.IsComplete());        
        }
        catch (SolverEngineInvalidException ex)
        {
            Logger.WriteLine("INVALID:");
            Logger.WriteLine(ex.ToString());
            throw;
        }
    }

}