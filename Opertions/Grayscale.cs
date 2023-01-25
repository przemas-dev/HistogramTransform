namespace HistogramTransform;

public class Grayscale : OperationStep
{
    public ImageData Input { get; set; }
    public ImageData Output { get; set; }
    public Grayscale(ImageData input)
    {
        Input = input;
        Output = new ImageData();
        Calculate();
    }

    public void Calculate()
    {
        var pixels = new byte[Input.Pixels.Length];
        var avgs = new int[256];

        for (var i = 0; i < pixels.Length; i+=4)
        {
            var avg = (byte)((Input.Pixels[i] + Input.Pixels[i + 1] + Input.Pixels[i + 2]) / 3);
            pixels[i] = avg;
            pixels[i + 1] = avg;
            pixels[i + 2] = avg;
            pixels[i + 3] = Input.Pixels[i + 3];
            avgs[avg]++;
        }
        
        Output.Red = avgs;
        Output.Green = avgs;
        Output.Blue = avgs;
        Output.Alpha = (int[])Input.Alpha.Clone();
        Output.Pixels = pixels;
        Output.Luminescence = avgs;
        Output.Values = avgs;
        Output.Width = Input.Width;
        Output.Height = Input.Height;
    }
}