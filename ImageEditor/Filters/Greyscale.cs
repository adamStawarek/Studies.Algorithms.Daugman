using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using ImageEditor.Filters.Interfaces;

namespace ImageEditor.Filters
{
    public class Greyscale:IFilter
    {
        public Bitmap Filter(Bitmap processedBitmap)
        {
            unsafe
            {
                BitmapData bitmapData = processedBitmap.LockBits(new Rectangle(0, 0, processedBitmap.Width, processedBitmap.Height), ImageLockMode.ReadWrite, processedBitmap.PixelFormat);

                int bytesPerPixel = Image.GetPixelFormatSize(processedBitmap.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* ptrFirstPixel = (byte*)bitmapData.Scan0;

                Parallel.For(0, heightInPixels, y =>
                {
                    byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);
                    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        int oldBlue = currentLine[x];
                        int oldGreen = currentLine[x + 1];
                        int oldRed = currentLine[x + 2];
                        var greyscale = (oldRed + oldBlue + oldGreen) / 3;
                        currentLine[x] = (byte)greyscale;
                        currentLine[x + 1] = (byte)greyscale;
                        currentLine[x + 2] = (byte)greyscale;
                    }
                });
                processedBitmap.UnlockBits(bitmapData);
                return processedBitmap;
            }
        }
        public string Name => "Greyscale";
    }
}