using ImageEditor.Filters.Interfaces;
using ImageEditor.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ImageEditor.Filters
{
    public struct CircleIntensity
    {
        public int radius;
        public int diffIntensity;
    }
    public class Daugman : IFilter
    {
        public string Name => "Daugman";
        private int _maxRadius = 80;

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
            var pointMaxIntensityDifferences = new Dictionary<Point, CircleIntensity>();

            foreach (var point in localMinPixels)
            {
                var previousIntensities = 0;
                pointMaxIntensityDifferences.Add(point, new CircleIntensity());
                for (int i = 25; i < _maxRadius; i++)
                {
                    var circularPoints = point.GetCircularPoints(i, Math.PI / 6.0f);

                    var intensitiesSum = (int)circularPoints.Sum(p =>
                    {
                        var pixel = processedBitmap.GetPixel(p.X, p.Y);
                        return GetGreyscale(pixel.R, pixel.G, pixel.B);
                    });
                    LogService.Write($"Intensity at radius {i} at point {point.X} , {point.Y} : {intensitiesSum}");
                    if (previousIntensities != 0)
                    {
                        var difference = Math.Abs(intensitiesSum - previousIntensities);
                        if (difference > pointMaxIntensityDifferences[point].diffIntensity)
                        {
                            pointMaxIntensityDifferences[point]=new CircleIntensity(){diffIntensity = difference,radius = i};
                        }
                    
                    }

                    previousIntensities = intensitiesSum;
                }
            }

            #endregion

            #region display obtained points

            var point2 = pointMaxIntensityDifferences.OrderByDescending(p => p.Value.diffIntensity).First();
            LogService.Write($"Selected center: {point2.Key.X} , {point2.Key.Y} ,radius: {point2.Value.radius} , " +
                             $"diffIntensity: {point2.Value.diffIntensity}");


            processedBitmap.SetPixel(point2.Key.X, point2.Key.Y, Color.Yellow);
            processedBitmap.SetPixel(point2.Key.X - 1, point2.Key.Y, Color.Yellow);
            processedBitmap.SetPixel(point2.Key.X + 1, point2.Key.Y, Color.Yellow);
            processedBitmap.SetPixel(point2.Key.X - 1, point2.Key.Y + 1, Color.Yellow);
            processedBitmap.SetPixel(point2.Key.X, point2.Key.Y + 1, Color.Yellow);
            processedBitmap.SetPixel(point2.Key.X + 1, point2.Key.Y + 1, Color.Yellow);
            processedBitmap.SetPixel(point2.Key.X - 1, point2.Key.Y - 1, Color.Yellow);
            processedBitmap.SetPixel(point2.Key.X, point2.Key.Y - 1, Color.Yellow);
            processedBitmap.SetPixel(point2.Key.X + 1, point2.Key.Y - 1, Color.Yellow);
            foreach (var p in point2.Key.GetCircularPoints(point2.Value.radius, Math.PI / 6.0f))
            {
                processedBitmap.SetPixel(p.X, p.Y, Color.Red);
                processedBitmap.SetPixel(p.X - 1, p.Y, Color.Red);
                processedBitmap.SetPixel(p.X + 1, p.Y, Color.Red);
                processedBitmap.SetPixel(p.X - 1, p.Y + 1, Color.Red);
                processedBitmap.SetPixel(p.X, p.Y + 1, Color.Red);
                processedBitmap.SetPixel(p.X + 1, p.Y + 1, Color.Red);
                processedBitmap.SetPixel(p.X - 1, p.Y - 1, Color.Red);
                processedBitmap.SetPixel(p.X, p.Y - 1, Color.Red);
                processedBitmap.SetPixel(p.X + 1, p.Y - 1, Color.Red);
            }
           


            return processedBitmap;

            #endregion
        }


        private byte GetGreyscale(byte r, byte g, byte b)
        {
            return (byte)((r + b + g) / 3);
        }
    }
}
