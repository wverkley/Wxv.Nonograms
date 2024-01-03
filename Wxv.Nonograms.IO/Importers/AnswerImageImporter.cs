using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Wxv.Core;
using Wxv.Nonograms.Core;

namespace Wxv.Nonograms.IO.Importers;

/// <summary>
/// Importer for answer images from www.nonograms.org 
/// </summary>
public class AnswerImageImporter : INonogrameImporter
{
    public string Name => "Answer Image";

    public Task<NonogrameImporterResult> TryImportAsync(object data, CancellationToken cancellationToken, string[]? description)
    {
        var result = new NonogrameImporterResult
        {
            IsSuccess = false,
            Solution = null
        };

        var bitmapBytes = data as byte[];
        if (bitmapBytes == null)
            return Task.FromResult(result);
        var bitmapFrame = BitmapFrame.Create(new MemoryStream(bitmapBytes));

        // get the pixels and dimensions
        var pixels = bitmapFrame.GetPixels();
        var width = pixels.GetLength(0);
        var height = pixels.GetLength(1);
        if (width < 8 || height < 8)
            return Task.FromResult(result);
        
        // figure out the number of horizonal cells by looking for 3 vertical black lines 3 pixels down
        // and then looking between the amount of white cells between the 2cnd and 3rd 
        var horizontalConsecutives = Enumerable.Range(0, width).Select(x => pixels[x, 3]).GetConsecutive().ToArray();
        if (horizontalConsecutives.Count(hc => IsBlack(hc.Value)) != 3)
            return Task.FromResult(result);
        var horizontalSolutionConsecutiveCells = horizontalConsecutives
            .SkipWhile(hc => !IsBlack(hc.Value))
            .SkipWhile(hc => IsBlack(hc.Value))
            .SkipWhile(hc => !IsBlack(hc.Value))
            .SkipWhile(hc => IsBlack(hc.Value))
            .TakeWhile(hc => !IsBlack(hc.Value))
            .Where(hc => IsWhite(hc.Value))
            .ToArray();
        var cellsWidth = horizontalSolutionConsecutiveCells.Length;
        if (cellsWidth < Constants.MinWidth || cellsWidth > Constants.MaxWidth)
            return Task.FromResult(result);

        // figure out the number of vertical cells by looking for 3 horizontal black lines 3 pixels across
        // and then looking between the amount of white cells between the 2cnd and 3rd 
        var verticalConsecutives = Enumerable.Range(0, height).Select(y => pixels[3, y]).GetConsecutive().ToArray();
        if (verticalConsecutives.Count(vc => IsBlack(vc.Value)) != 3)
            return Task.FromResult(result);
        var verticalSolutionConsecutiveCells = verticalConsecutives
            .SkipWhile(vc => !IsBlack(vc.Value))
            .SkipWhile(vc => IsBlack(vc.Value))
            .SkipWhile(vc => !IsBlack(vc.Value))
            .SkipWhile(vc => IsBlack(vc.Value))
            .TakeWhile(vc => !IsBlack(vc.Value))
            .Where(vc => IsWhite(vc.Value))
            .ToArray();
        var cellsHeight = verticalSolutionConsecutiveCells.Length;
        if (cellsHeight < Constants.MinHeight || cellsHeight > Constants.MaxHeight)
            return Task.FromResult(result);

        // where are the positions of the horizontal and vertical cells?
        var horizontalXs = horizontalSolutionConsecutiveCells.Select(hc => hc.Index + 1).ToArray();
        var verticalYs = verticalSolutionConsecutiveCells.Select(vc => vc.Index + 1).ToArray();

        // build a cell grid by looking for black cells
        var cells = new Grid<Cell>(cellsWidth, cellsHeight, (xi, yi) =>
            IsBlack(pixels[horizontalXs[xi] + 1, verticalYs[yi] + 1]) ? Cell.Set : Cell.Unset
        );

        result.IsSuccess = true;
        result.Solution = new Solution(cells, description ?? new [] { "From www.nonograms.org" });
        return Task.FromResult(result);
    }

    private bool IsBlack(BitmapSourceExtensions.PixelColor pixelColor)
        => pixelColor is { Red: < 64, Green: < 64, Blue: < 64 };

    private bool IsWhite(BitmapSourceExtensions.PixelColor pixelColor)
        => pixelColor is { Red: > 192, Green: > 192, Blue: > 192 };
}