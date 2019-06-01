using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageEditor.Helpers
{
    public static class BitmapExtensions
    {
        public static byte[] ToByteArray(this Bitmap bitmap)
        {
            BitmapData pdata = null;
            try
            {
                pdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                var length = pdata.Stride * bitmap.Height;
                var bytes = new byte[length];
                var ptr = pdata.Scan0;

                Marshal.Copy(ptr, bytes, 0, length);

                return bytes;
            }
            finally
            {
                if (pdata != null)
                    bitmap.UnlockBits(pdata);
            }

        }
    }
}