using System;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Wxv.Nonograms.IO;

// Credit Ray Burns : https://stackoverflow.com/a/1740553
internal static class BitmapSourceExtensions
{
    [StructLayout(LayoutKind.Explicit)]
    public struct PixelColor
    {
        // 32 bit BGRA 
        [FieldOffset(0)] public UInt32 BGRA;
        // 8 bit components
        [FieldOffset(0)] public byte Blue;
        [FieldOffset(1)] public byte Green;
        [FieldOffset(2)] public byte Red;
        [FieldOffset(3)] public byte Alpha;
        public int Grey => (Blue + Green + Red) / 3;

        public static bool Equals(PixelColor color0, PixelColor color1) => color0.BGRA == color1.BGRA;
        public override bool Equals(object? obj) => obj is PixelColor other && Equals(this, other);
        public override int GetHashCode() => BGRA.GetHashCode();
    }

    public static PixelColor[,] GetPixels(this BitmapSource source)
    {
        if(source.Format!=PixelFormats.Bgra32)
            source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);

        var width = source.PixelWidth;
        var height = source.PixelHeight;
        var result = new PixelColor[width, height];

        CopyPixels(source, result, width * 4, 0);
        return result;
    }    
    
    private static void CopyPixels(this BitmapSource source, PixelColor[,] pixels, int stride, int offset)
    {
        var height = source.PixelHeight;
        var width = source.PixelWidth;
        var pixelBytes = new byte[height * width * 4];
        source.CopyPixels(pixelBytes, stride, 0);
        var y0 = offset / width;
        var x0 = offset - width * y0;
        for(var y = 0; y < height; y++)
        for(var x = 0; x < width; x++)
            pixels[x+x0, y+y0] = new PixelColor
            {
                Blue  = pixelBytes[(y * width + x) * 4 + 0],
                Green = pixelBytes[(y * width + x) * 4 + 1],
                Red   = pixelBytes[(y * width + x) * 4 + 2],
                Alpha = pixelBytes[(y * width + x) * 4 + 3],
            };
    }
}