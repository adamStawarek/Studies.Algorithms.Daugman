using ImageEditor.Filters.Interfaces;
using ImageEditor.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ImageEditor.Filters
{
    public class Daugman : IFilter
    {
        public string Name => "Daugman";
        private int _maxRadius = 60;

        public Bitmap Filter(Bitmap processedBitmap)
        {
            LogService.Write("Thersholding started...");
            #region Thersholding: selecting potential center iris/pupil center pixels

            var thresholdPixels = new List<Point>();
            var thresholdValue = byte.MaxValue / 2;
            for (int y = 0; y < processedBitmap.Height; y++)
            {

                for (int x = 0; x < processedBitmap.Width; x++)
                {
                    var pixel = processedBitmap.GetPixel(x, y);
                    byte blue = pixel.R;
                    byte green = pixel.G;
                    byte red = pixel.B;
                    var grayScale = GetGreyscale(red, green, blue);
                    if (grayScale < thresholdValue)
                        thresholdPixels.Add(new Point(x, y));
                }
            }

            #endregion

            LogService.Write("Identifying local minimum...");
            #region Identifying local minimum point in 3x3 neighborhood

            var localMinPixels = new List<Point>();
            //remove edge points and point out of the image
            thresholdPixels = thresholdPixels.Where(p =>
                p.X > 1 && p.Y > 1 && p.X < processedBitmap.Width - 2 && p.Y < processedBitmap.Height - 2).ToList();

            foreach (var p1 in thresholdPixels)
            {
                var pointsInNeighborhood = p1.GetPointListAroundTheCenter(1);

                var isLocalMin = true;
                var pixel1 = processedBitmap.GetPixel(p1.X, p1.Y);
                var val1 = GetGreyscale(pixel1.R, pixel1.G, pixel1.B);
                foreach (var p2 in pointsInNeighborhood)
                {
                    var pixel2 = processedBitmap.GetPixel(p2.X, p2.Y);
                    var val2 = GetGreyscale(pixel2.R, pixel2.G, pixel2.B);
                    if (val1 > val2) isLocalMin = false;
                }

                if (isLocalMin)
                    localMinPixels.Add(p1);
            }

            //remove points that are closer that _minRadius to image edges
            localMinPixels = localMinPixels.Where(p => p.X + _maxRadius < processedBitmap.Width
                                                       && p.X - _maxRadius > 0
                                                       && p.Y + _maxRadius < processedBitmap.Height
                                                       && p.Y - _maxRadius > 0).ToList();

            #endregion

            LogService.Write("calculating circular pixels intensities");
            #region calculating circular pixels intensities
            var pointMaxIntensityDifferences = new Dictionary<Point, int>();

            foreach (var point in localMinPixels)
            {
                var previousIntensities = 0;
                pointMaxIntensityDifferences.Add(point, 0);
                for (int i = 1; i < _maxRadius; i++)
                {
                    var circularPoints = point.GetCircularPoints(i, Math.PI / 8.0f);

                    var intensitiesSum = circularPoints.Sum(p =>
                    {
                        var pixel = processedBitmap.GetPixel(p.X, p.Y);
                        return GetGreyscale(pixel.R, pixel.G, pixel.B);
                    });

                    if (previousIntensities != 0)
                    {
                        var difference = intensitiesSum - previousIntensities;
                        pointMaxIntensityDifferences[point] = difference > pointMaxIntensityDifferences[point]
                            ? difference
                            : pointMaxIntensityDifferences[point];
                    }

                    previousIntensities = intensitiesSum;
                }
            }

            #endregion

            #region display obtained points

            var point2 = pointMaxIntensityDifferences.OrderByDescending(p => p.Value).First();


            processedBitmap.SetPixel(point2.Key.X, point2.Key.Y, Color.Red);
            processedBitmap.SetPixel(point2.Key.X - 1, point2.Key.Y, Color.Red);
            processedBitmap.SetPixel(point2.Key.X + 1, point2.Key.Y, Color.Red);
            processedBitmap.SetPixel(point2.Key.X - 1, point2.Key.Y + 1, Color.Red);
            processedBitmap.SetPixel(point2.Key.X, point2.Key.Y + 1, Color.Red);
            processedBitmap.SetPixel(point2.Key.X + 1, point2.Key.Y + 1, Color.Red);
            processedBitmap.SetPixel(point2.Key.X - 1, point2.Key.Y - 1, Color.Red);
            processedBitmap.SetPixel(point2.Key.X, point2.Key.Y - 1, Color.Red);
            processedBitmap.SetPixel(point2.Key.X + 1, point2.Key.Y - 1, Color.Red);


            return processedBitmap;

            #endregion
        }


        private byte GetGreyscale(byte r, byte g, byte b)
        {
            return (byte)((r + b + g) / 3);
        }
    }
}
