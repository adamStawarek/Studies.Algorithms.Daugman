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
        private Bitmap _processedBitmap;

        public Bitmap Filter(Bitmap bitmap)
        {
            _processedBitmap = bitmap;
            var thresholdPixels = ApplyThersholding();
            var localMinPixels = FindLocalMinimums(ref thresholdPixels);
            var pointMaxIntensityDifferences = CalculateCircularPixelsIntensities(localMinPixels);
            MarkObtainedPoints(pointMaxIntensityDifferences);

            return _processedBitmap;
        }

        private List<Point> ApplyThersholding()
        {
            LogService.Write("Thersholding started...");

            var thresholdPixels = new List<Point>();
            var thresholdValue = byte.MaxValue / 2;
            for (int y = 0; y < _processedBitmap.Height; y++)
            {

                for (int x = 0; x < _processedBitmap.Width; x++)
                {
                    var pixel = _processedBitmap.GetPixel(x, y);
                    byte blue = pixel.R;
                    byte green = pixel.G;
                    byte red = pixel.B;
                    var grayScale = GetGreyscale(red, green, blue);
                    if (grayScale < thresholdValue)
                        thresholdPixels.Add(new Point(x, y));
                }
            }

            return thresholdPixels;
        }

        private List<Point> FindLocalMinimums(ref List<Point> thresholdPixels)
        {
            LogService.Write("Identifying local minimum point in 3x3 neighborhood");
          
            var localMinPixels = new List<Point>();
            //remove edge points and point out of the image
            thresholdPixels = thresholdPixels.Where(p =>
                p.X > 1 && p.Y > 1 && p.X < _processedBitmap.Width - 2 && p.Y < _processedBitmap.Height - 2).ToList();

            foreach (var p1 in thresholdPixels)
            {
                var pointsInNeighborhood = p1.GetPointListAroundTheCenter(1);

                var isLocalMin = true;
                var pixel1 = _processedBitmap.GetPixel(p1.X, p1.Y);
                var val1 = GetGreyscale(pixel1.R, pixel1.G, pixel1.B);
                foreach (var p2 in pointsInNeighborhood)
                {
                    var pixel2 = _processedBitmap.GetPixel(p2.X, p2.Y);
                    var val2 = GetGreyscale(pixel2.R, pixel2.G, pixel2.B);
                    if (val1 > val2) isLocalMin = false;
                }

                if (isLocalMin)
                    localMinPixels.Add(p1);
            }

            //remove points that are closer that _minRadius to image edges
            localMinPixels = localMinPixels.Where(p => p.X + _maxRadius < _processedBitmap.Width
                                                       && p.X - _maxRadius > 0
                                                       && p.Y + _maxRadius < _processedBitmap.Height
                                                       && p.Y - _maxRadius > 0).ToList();

            return localMinPixels;
        }       

        private Dictionary<Point, CircleIntensity> CalculateCircularPixelsIntensities(List<Point> localMinPixels)
        {
            LogService.Write("calculating circular pixels intensities");
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
                        var pixel = _processedBitmap.GetPixel(p.X, p.Y);
                        return GetGreyscale(pixel.R, pixel.G, pixel.B);
                    });
                    LogService.Write($"Intensity at radius {i} at point {point.X} , {point.Y} : {intensitiesSum}");
                    if (previousIntensities != 0)
                    {
                        var difference = Math.Abs(intensitiesSum - previousIntensities);
                        if (difference > pointMaxIntensityDifferences[point].DiffIntensity)
                        {
                            pointMaxIntensityDifferences[point] = new CircleIntensity() { DiffIntensity = difference, Radius = i };
                        }

                    }

                    previousIntensities = intensitiesSum;
                }
            }

            return pointMaxIntensityDifferences;
        }

        private void MarkObtainedPoints(Dictionary<Point, CircleIntensity> pointMaxIntensityDifferences)
        {
            var point2 = pointMaxIntensityDifferences.OrderByDescending(p => p.Value.DiffIntensity).First();
            LogService.Write($"Selected center: {point2.Key.X} , {point2.Key.Y} ,radius: {point2.Value.Radius} , " +
                             $"diffIntensity: {point2.Value.DiffIntensity}");


            _processedBitmap.SetPixel(point2.Key.X, point2.Key.Y, Color.Yellow);
            _processedBitmap.SetPixel(point2.Key.X - 1, point2.Key.Y, Color.Yellow);
            _processedBitmap.SetPixel(point2.Key.X + 1, point2.Key.Y, Color.Yellow);
            _processedBitmap.SetPixel(point2.Key.X - 1, point2.Key.Y + 1, Color.Yellow);
            _processedBitmap.SetPixel(point2.Key.X, point2.Key.Y + 1, Color.Yellow);
            _processedBitmap.SetPixel(point2.Key.X + 1, point2.Key.Y + 1, Color.Yellow);
            _processedBitmap.SetPixel(point2.Key.X - 1, point2.Key.Y - 1, Color.Yellow);
            _processedBitmap.SetPixel(point2.Key.X, point2.Key.Y - 1, Color.Yellow);
            _processedBitmap.SetPixel(point2.Key.X + 1, point2.Key.Y - 1, Color.Yellow);
            foreach (var p in point2.Key.GetCircularPoints(point2.Value.Radius, Math.PI / 6.0f))
            {
                _processedBitmap.SetPixel(p.X, p.Y, Color.Red);
                _processedBitmap.SetPixel(p.X - 1, p.Y, Color.Red);
                _processedBitmap.SetPixel(p.X + 1, p.Y, Color.Red);
                _processedBitmap.SetPixel(p.X - 1, p.Y + 1, Color.Red);
                _processedBitmap.SetPixel(p.X, p.Y + 1, Color.Red);
                _processedBitmap.SetPixel(p.X + 1, p.Y + 1, Color.Red);
                _processedBitmap.SetPixel(p.X - 1, p.Y - 1, Color.Red);
                _processedBitmap.SetPixel(p.X, p.Y - 1, Color.Red);
                _processedBitmap.SetPixel(p.X + 1, p.Y - 1, Color.Red);
            }
        }

        private byte GetGreyscale(byte r, byte g, byte b)
        {
            return (byte)((r + b + g) / 3);
        }
    }
}
