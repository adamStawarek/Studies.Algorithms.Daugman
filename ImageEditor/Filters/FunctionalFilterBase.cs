using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using ImageEditor.Filters.Interfaces;

namespace ImageEditor.Filters
{

    /// <summary>
    /// source: http://csharpexamples.com/fast-image-processing-c/
    /// </summary>
    public abstract class FunctionalFilterBase:IFilter
    {
        public abstract string Name { get; }
        public abstract byte Transform(byte rgbVal);
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

                        currentLine[x] = Transform((byte)oldBlue);
                        currentLine[x + 1] = Transform((byte)oldGreen);
                        currentLine[x + 2] = Transform((byte)oldRed);
                    }
                });
                processedBitmap.UnlockBits(bitmapData);
                return processedBitmap;
            }
        }       
    }
}