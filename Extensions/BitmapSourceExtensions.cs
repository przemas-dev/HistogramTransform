using System.Linq;
using System.Windows.Media.Imaging;

namespace HistogramTransform;

public static class BitmapSourceExtensions
{
    public static ImageData GetOperationParams(this BitmapSource bitmapSource)
    {
        var red = new int[256];
        var green = new int[256];
        var blue = new int[256];
        var alpha = new int[256];
        var values = new int[256];
        var luma = new int[256];
        var stride = bitmapSource.PixelWidth * ((bitmapSource.Format.BitsPerPixel + 7) / 8);
        var size = bitmapSource.PixelHeight * stride;
        var pixels = new byte[size];
        bitmapSource.CopyPixels(pixels, stride, 0);
        
        for (var i = 0; i < pixels.Length; i += 4)
        {
            blue[pixels[i]]++;
            green[pixels[i + 1]]++;
            red[pixels[i + 2]]++;
            alpha[pixels[i + 3]]++;
            int value = new[] { pixels[i], pixels[i + 1], pixels[i + 2]}.Max();
            values[value]++;
            var lum = Helpers.LuminescenceFunc(pixels[i + 2], pixels[i + 1], pixels[i]);
            luma[lum]++;
        }

        return new ImageData()
        {
            Red = red,
            Green = green,
            Blue = blue,
            Alpha = alpha,
            Pixels = pixels,
            Luminescence = luma,
            Values = values,
            Width = bitmapSource.PixelWidth,
            Height = bitmapSource.PixelHeight
        };
    }
}