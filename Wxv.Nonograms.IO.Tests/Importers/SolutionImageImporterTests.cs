using Wxv.Core;
using Wxv.Nonograms.IO.Importers;
using Xunit.Abstractions;

namespace Wxv.Nonograms.IO.Tests.Importers;

public class SolutionImageImporterTests
{
    private ITestOutputHelper Logger { get; }
    
    public SolutionImageImporterTests(ITestOutputHelper logger)
    {
        Logger = logger;
    }

    [Theory]
    [InlineData("Image1.png", 5, 7)]
    [InlineData("Image2.png", 10, 10)]
    public async Task Import(string filename, int expectedWidth, int expectedHeight)
    {
        await using var stream = typeof(AnswerImageImporterTests).Assembly.GetManifestResourceStream(typeof(Tests).Namespace + ".SolutionImages." + filename);
        var data = await stream!.ReadAllBytes();

        var imageImporter = new SolutionImageImporter();
        var result = await imageImporter.TryImportAsync(data, CancellationToken.None);
        
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedWidth, result.Solution!.Width);
        Assert.Equal(expectedHeight, result.Solution!.Height);
        
        Logger.WriteLine(result.Solution!.ToString());
    }
}