namespace HistogramTransform;

public static class Helpers
{
    public static byte LuminescenceFunc(byte red, byte green, byte blue)
    {
        return (byte)(0.2126 * red + 0.7152 * green + 0.0722 * blue);
    }
}