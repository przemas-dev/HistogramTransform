using System.Collections.Generic;
using System.Linq;

namespace HistogramTransform;

public class Equalization : OperationStep
{
    public ImageData Input { get; set; }
    public ImageData Output { get; set; }
    
    public Equalization(ImageData input)
    {
        Input = input;
        Output = new ImageData();
        Calculate();
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


    public void Calculate()
    {
        var LUTred = CalculateLUT(Input.Red, Input.Size);
        var LUTgreen = CalculateLUT(Input.Green, Input.Size);
        var LUTblue = CalculateLUT(Input.Blue, Input.Size);
        
        var red = new int[256];
        var green = new int[256];
        var blue = new int[256];
        var alpha = new int[256];
        var pixels = new byte[Input.Pixels.Length];
        var luminescence = new int[256];
        var values = new int[256];

        for (var i = 0; i < pixels.Length; i+=4)
        {
            pixels[i] = LUTblue[Input.Pixels[i]];
            pixels[i + 1] = LUTgreen[Input.Pixels[i + 1]];
            pixels[i + 2] = LUTred[Input.Pixels[i + 2]];
            pixels[i + 3] = Input.Pixels[i + 3];
            blue[pixels[i]]++;
            green[pixels[i + 1]]++;
            red[pixels[i + 2]]++;
            alpha[pixels[i + 3]]++;
            int value = new[] { pixels[i], pixels[i + 1], pixels[i + 2]}.Max();
            values[value]++;
            var lum = Helpers.LuminescenceFunc(pixels[i + 2], pixels[i + 1], pixels[i]);
            luminescence[lum]++;
        }
        
        Output.Red = red;
        Output.Green = green;
        Output.Blue = blue;
        Output.Alpha = alpha;
        Output.Pixels = pixels;
        Output.Luminescence = luminescence;
        Output.Values = values;
        Output.Width = Input.Width;
        Output.Height = Input.Height;
    }
}