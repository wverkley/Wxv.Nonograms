using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Wxv.Core;
using Wxv.Nonograms.Core;

namespace Wxv.Nonograms.IO.Importers;

public class SolutionImageImporter : INonogrameImporter
{
    public string Name => "Solution Image";

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
        if (width < Constants.MinWidth 
          || width > Constants.MaxWidth 
          || height < Constants.MinHeight 
          || height > Constants.MaxHeight)
            return Task.FromResult(result);

        var pixelGrid = new Grid<BitmapSourceExtensions.PixelColor>(
            width, 
            height, 
            (x, y) => pixels[x, y]);

        var distinctColors = pixelGrid.Distinct().OrderBy(pc => pc.Grey).ToArray();
        if (distinctColors.Length != 2)
            return Task.FromResult(result);

        var cells = new Grid<Cell>(
            width,
            height,
            (x, y) => BitmapSourceExtensions.PixelColor.Equals(pixelGrid[x, y], distinctColors.First())
                ? Cell.Set 
                : Cell.Unset);

        result.IsSuccess = true;
        result.Solution = new Solution(cells, description);
        return Task.FromResult(result);
    }

}