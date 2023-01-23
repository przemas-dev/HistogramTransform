using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HistogramTransform;

public class Equalization
{
    private BitmapSource _bitmapSource;
    private int _pixelCount;
    private int[] red;
    private int[] green;
    private int[] blue;
    private int[] alpha;
    private byte[] pixels;

    private byte[] LUTred;
    private byte[] LUTblue;
    private byte[] LUTgreen;

    public Equalization(BitmapSource bitmapSource)
    {
        _bitmapSource = bitmapSource;
        var size = bitmapSource.PixelWidth * bitmapSource.PixelHeight;
        red = new int[256];
        green = new int[256];
        blue = new int[256];
        alpha = new int[256];
        
        
        var stride = _bitmapSource.PixelWidth * 4;
        pixels = new byte[_bitmapSource.PixelHeight * stride];
        _bitmapSource.CopyPixels(pixels, stride, 0);
        for (var i = 0; i < pixels.Length; i += 4)
        {
            blue[pixels[i]]++;
            green[pixels[i + 1]]++;
            red[pixels[i + 2]]++;
            alpha[pixels[i + 3]]++;
        }
        
        
        LUTred = CalculateLUT(red, size);
        LUTgreen = CalculateLUT(green, size);
        LUTblue = CalculateLUT(blue, size);
        

        for (var i = 0; i < pixels.Length; i+=4)
        {
            pixels[i] = LUTblue[pixels[i]];
            pixels[i+1] = LUTgreen[pixels[i+1]];
            pixels[i+2] = LUTred[pixels[i+2]];
        }

    }
    
    private static byte[] CalculateLUT(IReadOnlyList<int> values, int size)
    {
        var minValue = values.First(value => value != 0);
        
        var lut = new byte[256];
        double sum = 0;
        for (var i = 0; i < 256; i++)
        {
            sum += values[i];
            lut[i] = (byte)(((sum - minValue) / (size - minValue)) * 255.0);
        }
 
        return lut;
    }
    
    public BitmapSource GetBitmapImage()
    {
        var stride = _bitmapSource.PixelWidth * 4;
        

        var bitmap = BitmapSource.Create(
            _bitmapSource.PixelWidth, _bitmapSource.PixelHeight, 96, 96, PixelFormats.Bgra32, null, pixels, stride);
        return BitmapFrame.Create(bitmap);
    }
    
    
}