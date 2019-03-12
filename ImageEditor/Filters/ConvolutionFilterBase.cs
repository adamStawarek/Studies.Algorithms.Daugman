using System;
using System.Collections.Generic;
using ImageEditor.Filters.Interfaces;
using System.Drawing;
using ImageEditor.Helpers;

namespace ImageEditor.Filters
{
    public abstract class ConvolutionFilterBase:IFilter
    {
        public abstract double[,] Matrix { get; }
        public abstract double Divisor { get; }
        public abstract double Bias { get; }
        public abstract string Name { get; }
        public Bitmap Filter(Bitmap image)
        {
            var pointColorDict = new Dictionary<Point, Color>();
            int filterOffset = (Matrix.GetLength(1) - 1) / 2;
            if (Matrix.GetLength(0) != Matrix.GetLength(1))
                throw new ArgumentException("Matrix dimension must be the same");

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    double blue = 0;
                    double green = 0;
                    double red = 0;
                    for (int filterY = -filterOffset, i = 0; filterY <= filterOffset; filterY++, i++)
                    {
                        for (int filterX = -filterOffset, j = 0; filterX <= filterOffset; filterX++, j++)
                        {
                            //handling edge pixels
                            var cordX = x + filterX;
                            var cordY = y + filterY;
                            if (cordX < 0 || cordX >= image.Width)
                                cordX = x;
                            if (cordY < 0 || cordY >= image.Height)
                                cordY = y;

                            var pixel = image.GetPixel(cordX, cordY);
                            red += pixel.R * Matrix[i, j];
                            green += pixel.G * Matrix[i, j];
                            blue += pixel.B * (Matrix[i, j]);
                        }
                    }

                    red = (int)(red / Divisor + Bias);
                    green = (int)(green / Divisor + Bias);
                    blue = (int)(blue / Divisor + Bias);
                    pointColorDict.Add(new Point(x, y), Color.FromArgb(red.TruncateRgb(), green.TruncateRgb(), blue.TruncateRgb()));
                }
            }

            foreach (var c in pointColorDict)
            {
                image.SetPixel(c.Key.X, c.Key.Y, c.Value);
            }

            return image;
        }
    }
}
