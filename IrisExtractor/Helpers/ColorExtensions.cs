using System.Drawing;

namespace ImageEditor.Helpers
{
    public static class ColorExtensions
    {
        public static byte GetGreyscale(this Color color)
        {
            return (byte)((color.R + color.G + color.B) / 3);
        }
    }
}