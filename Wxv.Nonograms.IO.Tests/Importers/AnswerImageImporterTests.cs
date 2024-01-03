using Wxv.Core;
using Wxv.Nonograms.IO.Importers;
using Xunit.Abstractions;

namespace Wxv.Nonograms.IO.Tests.Importers;

public class AnswerImageImporterTests
{
    private ITestOutputHelper Logger { get; }
    
    public AnswerImageImporterTests(ITestOutputHelper logger)
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
    [InlineData("Image7.png", 40, 45)]
    [InlineData("Image8.png", 20, 8)]
    [InlineData("Image9.png", 15, 15)]
    [InlineData("Image10.png", 20, 20)]
    public async Task Import(string filename, int expectedWidth, int expectedHeight)
    {
        await using var stream = typeof(AnswerImageImporterTests).Assembly.GetManifestResourceStream(typeof(Tests).Namespace + ".AnswerImages." + filename);
        var data = await stream!.ReadAllBytes();

        var imageImporter = new AnswerImageImporter();
        var result = await imageImporter.TryImportAsync(data, CancellationToken.None);
        
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedWidth, result.Solution!.Width);
        Assert.Equal(expectedHeight, result.Solution!.Height);
        
        Logger.WriteLine(result.Solution!.ToString());
    }
}