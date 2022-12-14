using OpenTK.Graphics;
using System.Drawing;

namespace StorybrewScripts.Midori.Image
{
    public class LockedBitmap
    {
        public Color4[,] pixels;

        public Color4 this[int i, int j]
        {
            get { return pixels[i, j]; }
            set { pixels[i, j] = value; }
        }

        public int Width => pixels.GetLength(0);
        public int Height => pixels.GetLength(1);

        public LockedBitmap(Bitmap bmp)
        {
            ImportBitmap(bmp);
        }

        public void ImportBitmap(Bitmap bmp)
        {
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var bitmapData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

            var ptr = bitmapData.Scan0;
            var bytesPerPixel = Bitmap.GetPixelFormatSize(bmp.PixelFormat) / 8;
            var byteCount = bitmapData.Stride * bmp.Height;
            var rgbValues = new byte[byteCount];
            int pixelsHeight = bitmapData.Height;
            int bytesWidth = bitmapData.Width * bytesPerPixel;

            this.pixels = new Color4[bmp.Width, bmp.Height];
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, byteCount);
            bmp.UnlockBits(bitmapData);

            for (int y = 0; y < pixelsHeight; y++)
            {
                var lineOffset = y * bitmapData.Stride;
                for (int x = 0; x < bytesWidth; x += bytesPerPixel)
                {
                    var color = new Color4(rgbValues[lineOffset + x + 2], rgbValues[lineOffset + x + 1], rgbValues[lineOffset + x], (byte)((bytesPerPixel % 4 == 0) ? rgbValues[lineOffset + x + 3] : 255));
                    pixels[x / bytesPerPixel, y] = color;
                }
            }
        }

        public Bitmap ExportBitmap()
        {
            var rect = new Rectangle(0, 0, pixels.GetLength(0), pixels.GetLength(1));
            var bmp = new Bitmap(pixels.GetLength(0), pixels.GetLength(1));
            var bitmapData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

            var ptr = bitmapData.Scan0;
            var bytesPerPixel = Bitmap.GetPixelFormatSize(bmp.PixelFormat) / 8;
            var byteCount = bitmapData.Stride * bmp.Height;
            var rgbValues = new byte[byteCount];
            int pixelsHeight = bitmapData.Height;
            int bytesWidth = bitmapData.Width * bytesPerPixel;

            for (int y = 0; y < pixelsHeight; y++)
            {
                var lineOffset = y * bitmapData.Stride;
                for (int x = 0; x < bytesWidth; x += bytesPerPixel)
                {
                    var color = pixels[x / bytesPerPixel, y];
                    if (bytesPerPixel % 4 == 0) rgbValues[lineOffset + x + 3] = (byte)(color.A * 255);
                    rgbValues[lineOffset + x + 2] = (byte)(color.R * 255);
                    rgbValues[lineOffset + x + 1] = (byte)(color.G * 255);
                    rgbValues[lineOffset + x] = (byte)(color.B * 255);
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, byteCount);
            bmp.UnlockBits(bitmapData);

            return bmp;
        }
    }
}
