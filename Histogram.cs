using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HistogramTransform
{
    public class Histogram
    {
        private BitmapSource _histogramBitmapSource;
        private int[] _countValues;
        private int _pixelCount;
        private Scale _scale;
        
        


        public Histogram(BitmapSource bitmapSource, Scale scale= Scale.Logarithmic)
        {
            _scale = scale;
            var stride = bitmapSource.PixelWidth * ((bitmapSource.Format.BitsPerPixel + 7) / 8);
            var size = bitmapSource.PixelHeight * stride;
            var pixels = new byte[size];
            bitmapSource.CopyPixels(pixels, stride, 0);
            
            _countValues = new int[256];
            _pixelCount = bitmapSource.PixelWidth * bitmapSource.PixelHeight;
            
            for (var i = 0; i < pixels.Length; i += 4)
            {
                var red = pixels[i + 2];
                var green = pixels[i + 1];
                var blue = pixels[i];
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

            var logs = _countValues.Select(value => Math.Log10(value)).ToArray();
            var maxLog = logs.Max();
            
            
            for (var x = 0; x < width; x++)
            {
                var barHeight = _scale switch
                {
                    Scale.Logarithmic => logs[x]*height/maxLog * 0.8,
                    Scale.Linear => _countValues[x] * height / maxCounts * 0.8,
                    _ => throw new ArgumentOutOfRangeException()
                };
                for (var y = 0; y < barHeight; y++)
                {
                    var index = (height - y - 1) * stride + x;
                    pixels[index] = 1;
                }
            }

            var colors = new List<Color>() { Colors.LightGray, Colors.Black, Colors.Red };
            var palette = new BitmapPalette(colors);

            _histogramBitmapSource = BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Indexed8, palette, pixels, stride);
            return _histogramBitmapSource;
        }

        public void SaveHistogramToFile(string fileName)
        {
            var jpg = new JpegBitmapEncoder();
            jpg.Frames.Add(BitmapFrame.Create(_histogramBitmapSource));
            try
            {
                using var fileStream = File.Create(fileName);
                jpg.Save(fileStream);
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message, "Błąd zapisu pliku",MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }

        public void ChangeScale(Scale selectedScale)
        {
            _scale = selectedScale;
        }
    }
    public enum Scale
    {
        Logarithmic,
        Linear
    }
}
