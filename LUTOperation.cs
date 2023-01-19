using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HistogramTransform;

public class LUTOperation
{
    private BitmapSource _bitmapSource;
    private int _pixelCount;
    private byte[] Red;
    private byte[] Green;
    private byte[] Blue;
    private byte[] Alpha;

    public LUTOperation(BitmapSource bitmapSource)
    {
        _bitmapSource = bitmapSource;
    }
    
    public BitmapFrame GetBitmapImage()
    {
        _pixelCount = _bitmapSource.PixelWidth * _bitmapSource.PixelWidth;
        Red = new byte[_pixelCount];
        Green = new byte[_pixelCount];
        Blue = new byte[_pixelCount];
        
        var stride = _bitmapSource.PixelWidth * 4;
        var pixels = new byte[_bitmapSource.PixelHeight * stride];
        _bitmapSource.CopyPixels(pixels, stride, 0);
        for (var i = 0; i < pixels.Length; i += 4)
        {
            Red[i / 4] = pixels[i];
            Green[i / 4] = pixels[i + 1];
            Blue[i / 4] = pixels[i + 2];
        }

        var redMax = Red.Max();
        var greenMax = Green.Max();
        var blueMax = Blue.Max();

        var redMin = Red.Min();
        var greenMin = Green.Min();
        var blueMin = Blue.Min();
        for (var i = 0; i < pixels.Length; i += 4)
        {
            pixels[i] = (byte)(255*(pixels[i] - redMin) / (redMax - redMin));
            pixels[i+1] = (byte)(255*(pixels[i+1] - greenMin) / (greenMax - greenMin));
            pixels[i+2] = (byte)(255*(pixels[i+2] - blueMin) / (blueMax - blueMin));
        }

        var bitmap = BitmapSource.Create(
            _bitmapSource.PixelWidth, _bitmapSource.PixelHeight, 96, 96, PixelFormats.Bgr32, null, pixels, stride);
        return BitmapFrame.Create(bitmap);
    }
}