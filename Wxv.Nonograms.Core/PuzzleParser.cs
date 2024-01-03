using Wxv.Core;
using Wxv.Parser;
using Wxv.Parser.IO;

namespace Wxv.Nonograms.Core;

public class PuzzleParser
{
    private static Lazy<ParserEngine> ParserEngine { get; } = new(() =>
    {
        var resourceName = typeof(PuzzleParser).FullName + ".txt";
        var source = typeof(PuzzleParser).Assembly.GetManifestResourceString(resourceName);
        return new ParserReader().Read(source);
    });

    public Puzzle Parse(string content)
    {
        var parserResult = ParserEngine.Value.Parse(content);
        if (!parserResult.IsMatch)
            throw new InvalidDataException();

        var horizontalLengths = parserResult
            .First("horizontal-lengths")
            .Where("lengths")
            .Select(lenghtsParserResult => lenghtsParserResult
                .Where("length")
                .Select(pr => int.Parse(pr.Value))
                .ToArray())
            .ToArray();
        
        var verticalLengths = parserResult
            .First("vertical-lengths")
            .Where("lengths")
            .Select(lenghtsParserResult => lenghtsParserResult
                .Where("length")
                .Select(pr => int.Parse(pr.Value))
                .ToArray())
            .ToArray();
        
        var width = verticalLengths.Length;
        var height = horizontalLengths.Length;
        if (width <= 0 || height <= 0)
            throw new InvalidDataException();

        if (
          horizontalLengths.Any(lengths => lengths.Any(length => length <= 0))
          || horizontalLengths.Any(lengths => lengths.Sum() + lengths.Length - 1 > width)
          )
            throw new InvalidDataException();

        if (
            verticalLengths.Any(lengths => lengths.Any(length => length <= 0))
            || verticalLengths.Any(lengths => lengths.Sum() + lengths.Length - 1 > height)
        )
            throw new InvalidDataException();

        return new Puzzle
        {
            Description = parserResult
                .Where("description")
                .Select(pr => pr.First("description-value").Value.TrimEnd())
                .ToArray(),
            HorizontalLengths = horizontalLengths,
            VerticalLengths = verticalLengths
        };
    }
}