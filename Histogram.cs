using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HistogramTransform
{
    public class Histogram
    {

        private BitmapSource _bitmapSource;
        private int[] _countValues;

        public Histogram(BitmapSource bitmapSource)
        {
            _bitmapSource = bitmapSource;
            _countValues = new int[256];
            var stride = _bitmapSource.PixelWidth * 4;
            var pixels = new byte[_bitmapSource.PixelHeight * stride];
            _bitmapSource.CopyPixels(pixels, stride, 0);
            for (var i = 0; i < pixels.Length; i += 4)
            {
                var red = pixels[i];
                var green = pixels[i + 1];
                var blue = pixels[i + 2];
                var alpha = pixels[i + 3];

                int value = new[] { red, green, blue }.Max();
                _countValues[value]++;
            }
        }

        public BitmapSource GetBitmapImage()
        {
            const int width = 256;
            const int height = 256;
            var stride = width; //Depends on used PixelFormat; with Indexed8 one pixel per one byte
            var pixels = new byte[height * stride];

            var maxCounts = _countValues.Max();

            for (var x = 0; x < width; x++)
            {
                var barHeight = _countValues[x] * height / maxCounts * 0.8;
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
    }
}
