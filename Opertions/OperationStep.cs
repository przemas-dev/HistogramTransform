using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HistogramTransform;

public interface OperationStep
{
    public ImageData Input { get; set; }
    public ImageData Output { get; set; }

    public void Calculate();
}

public class ImageData
{
    public int[] Red { get; set; }
    public int[] Blue { get; set; }
    public int[] Green { get; set; } 
    public int[] Alpha { get; set; }
    public int[] Values { get; set; }
    public int[] Luminescence { get; set; }
    public byte[] Pixels { get; set; }
    
    
    public int Width { get; set; }
    public int Height { get; set; }
    public int Size => Width * Height;

    public BitmapSource GetHistogramImage(HistogramDisplayChannel channel, Scale scale)
    {
        const int width = 256;
        const int height = 256;
        const int stride = width;
        var pixels = new byte[height * width];

        var counts = channel switch
        {
            HistogramDisplayChannel.Red => Red,
            HistogramDisplayChannel.Green => Green,
            HistogramDisplayChannel.Blue => Blue,
            HistogramDisplayChannel.Value => Values,
            HistogramDisplayChannel.RelativeLuminance => Luminescence,
            _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, null)
        };

        var values = scale switch
        {
            Scale.Logarithmic => counts.Select(count => Math.Log10(count)).ToArray(),
            Scale.Linear => counts.Select(Convert.ToDouble).ToArray(),
            _ => throw new ArgumentOutOfRangeException(nameof(scale), scale, null)
        };
        var maxValue = values.Max();

        for (var x = 0; x < width; x++)
        {
            var barHeight = values[x] * height / maxValue * 0.8;
            for (var y = 0; y < barHeight; y++)
            {
                var index = (height - y - 1) * stride + x;
                pixels[index] = 1;
            }
        }

        var colors = new List<Color>() { Colors.LightGray, Colors.Black, Colors.Red };
        var palette = new BitmapPalette(colors);

        var bitmapSource = BitmapSource.Create(
            width, height, 96, 96, PixelFormats.Indexed8, palette, pixels, stride);
        return bitmapSource;
    }

    public BitmapSource GetBitmapSource()
    {

        var bitmapSource = BitmapSource.Create(
            Width, Height, 96, 96, PixelFormats.Bgra32, null, Pixels, Width * 4);
        return bitmapSource;
    }
}

public enum HistogramDisplayChannel
{
    Red,
    Green,
    Blue,
    Value,
    RelativeLuminance
}