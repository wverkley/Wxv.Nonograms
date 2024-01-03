using Wxv.Core;
using Wxv.Nonograms.IO.Importers;

namespace Wxv.Nonograms.IO.Tests.Importers;

public class NonogramUrlImporterTests
{
    [Theory]
    [InlineData(
        "https://www.nonograms.org/nonograms/i/61475", 
        "61475.html", 
        "https://static.nonograms.org/files/nonograms/large/chtenie_knigi2_12_1_1p.png",
        "Image1.png")]
    [InlineData(
        "https://www.nonograms.org/nonograms/i/8543", 
        "8543.html", 
        "https://static.nonograms.org/files/nonograms/large/olen16_12_1_1p.png",
        "Image2.png")]
    public async Task Import(string nonogramUrl, string pageFilename, string answerImageUrl, string imageFilename)
    {
        await using var pageStream = typeof(AnswerImageImporterTests).Assembly.GetManifestResourceStream(typeof(Tests).Namespace + ".Pages." + pageFilename);
        var pageData = await pageStream!.ReadAllText();

        await using var imageStream = typeof(AnswerImageImporterTests).Assembly.GetManifestResourceStream(typeof(Tests).Namespace + ".AnswerImages." + imageFilename);
        var imageData = await imageStream!.ReadAllBytes();

        var nonogramUrlImporter = new NonogramUrlImporter(
            (_, _) => Task.FromResult(pageData),
            (url, _) =>
            {
                Assert.Equal(answerImageUrl, url);
                return Task.FromResult(imageData);
            });

        var result = await nonogramUrlImporter.TryImportAsync(nonogramUrl, CancellationToken.None);
        
        Assert.True(result.IsSuccess);
    }
}