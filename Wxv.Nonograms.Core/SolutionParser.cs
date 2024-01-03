using Wxv.Core;
using Wxv.Parser;
using Wxv.Parser.IO;

namespace Wxv.Nonograms.Core;

public class SolutionParser
{
    private static Lazy<ParserEngine> ParserEngine { get; } = new(() =>
    {
        var resourceName = typeof(SolutionParser).FullName + ".txt";
        var source = typeof(SolutionParser).Assembly.GetManifestResourceString(resourceName);
        return new ParserReader().Read(source);
    });

    public Solution Parse(string content)
    {
        var parserResult = ParserEngine.Value.Parse(content);
        if (!parserResult.IsMatch)
            throw new InvalidDataException();

        var height = parserResult.Where("row").Count();
        var width = parserResult.Where("row").Max(row => row.Where("cell").Count());
        var cells = new Grid<Cell>(width, height);
        
        parserResult
            .Where("row")
            .ForEach((rowParserResult, rowIndex) =>
            {
                rowParserResult
                    .Where("cell")
                    .ForEach((cellParserResult, columnIndex) => cells[columnIndex, rowIndex] = 
                        string.IsNullOrWhiteSpace(cellParserResult.Value)
                        ? Cell.Unset
                        : Cell.Set);
            });
        
        return new Solution
        {
            Description = parserResult
                .Where("description")
                .Select(pr => pr.First("description-value").Value.TrimEnd())
                .ToArray(),
            Cells = cells
        };
    }
    
    
}