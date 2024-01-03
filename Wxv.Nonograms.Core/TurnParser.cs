using Wxv.Core;
using Wxv.Parser;
using Wxv.Parser.IO;

namespace Wxv.Nonograms.Core;

public class TurnParser
{
    private static Lazy<ParserEngine> ParserEngine { get; } = new(() =>
    {
        var resourceName = typeof(TurnParser).FullName + ".txt";
        var source = typeof(TurnParser).Assembly.GetManifestResourceString(resourceName);
        return new ParserReader().Read(source);
    });

    public Turn Parse(string content)
    {
        var parserResult = ParserEngine.Value.Parse(content);
        if (!parserResult.IsMatch)
            throw new InvalidDataException();

        var height = parserResult.First("cells").Where("cellRow").Count();
        var width = parserResult.First("cells").Where("cellRow").Max(row => row.Where("cell").Count());
        var cells = new Grid<Cell>(width, height);
        var wrongCells = new Grid<bool>(width, height);
        var hintCells = new Grid<bool>(width, height);
        
        var horizontalLengthsCompleted = parserResult
            .First("horizontalCompleted")
            .Where("flagRow")
            .Select(prr => prr
                .Where("flag")
                .Select(prc => prc.Value.First().ParseFlag())
                .ToArray())
            .ToArray();
        if (horizontalLengthsCompleted.Length != cells.Height)
            throw new InvalidDataException();
        
        var verticalLengthsCompleted = parserResult
            .First("verticalCompleted")
            .Where("flagRow")
            .Select(prr => prr
                .Where("flag")
                .Select(prc => prc.Value.First().ParseFlag())
                .ToArray())
            .ToArray();
        if (verticalLengthsCompleted.Length != cells.Width)
            throw new InvalidDataException();
        
        parserResult
            .First("cells")
            .Where("cellRow")
            .ForEach((rowParserResult, rowIndex) =>
            {
                rowParserResult
                    .Where("cell")
                    .ForEach((cellParserResult, columnIndex) => cells[columnIndex, rowIndex] = cellParserResult.Value.First().ParseCell());
            });
        
        parserResult
            .First("wrongCells")
            .Where("flagRow")
            .ForEach((rowParserResult, rowIndex) =>
            {
                rowParserResult
                    .Where("flag")
                    .ForEach((cellParserResult, columnIndex) => wrongCells[columnIndex, rowIndex] = cellParserResult.Value.First().ParseFlag());
            });
        if (wrongCells.Width != cells.Width || wrongCells.Height != cells.Height)
            throw new InvalidDataException();
        
        parserResult
            .First("hintCells")
            .Where("flagRow")
            .ForEach((rowParserResult, rowIndex) =>
            {
                rowParserResult
                    .Where("flag")
                    .ForEach((cellParserResult, columnIndex) => hintCells[columnIndex, rowIndex] = cellParserResult.Value.First().ParseFlag());
            });
        if (hintCells.Width != cells.Width || hintCells.Height != cells.Height)
            throw new InvalidDataException();

        var isFinished = string.Equals(parserResult
            .FirstOrDefault("isFinished")
            ?.FirstOrDefault("isFinishedFlag") 
            ?.Value, CellExtensions.IsFinishedCharacter, StringComparison.OrdinalIgnoreCase);
        
        return new Turn
        {
            HorizontalLengthsCompleted = horizontalLengthsCompleted,
            VerticalLengthsCompleted = verticalLengthsCompleted,
            Cells = cells,
            WrongCells = wrongCells,
            HintCells = hintCells,
            IsFinished = isFinished
        };
    }
    
    
}