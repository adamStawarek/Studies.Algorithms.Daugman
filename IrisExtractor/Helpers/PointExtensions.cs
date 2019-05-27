using System;
using System.Collections.Generic;
using System.Windows;
using Point = System.Drawing.Point;

namespace ImageEditor.Helpers
{
    public static class PointExtensions
    {
        public static List<Point> GetCircularPoints(this Point center,double radius, double angleInterval)
        {
            List<Point> points = new List<Point>();

            for (double interval = 0; interval < 2 * Math.PI; interval += angleInterval)
            {
                int X = (int)(center.X + (radius * Math.Cos(interval)));
                int Y = (int)(center.Y + (radius * Math.Sin(interval)));

                points.Add(new Point(X, Y));
            }

            points.RemoveRange(5, 8);
            points.RemoveRange(14, 8);
            return points;
        }

        public static double GetAngle(this Point p1, Point p2)
        {
            float xDiff = p2.X - p1.X;
            float yDiff = p2.Y - p1.Y;
            return Math.Atan2(yDiff, xDiff) * (180 / Math.PI);
        }

        public static Point PolarToCartesian(double angle, double radius)
        {
            var angleRad = (Math.PI / 180.0) * angle;
            var x = (int)Math.Round(radius * Math.Cos(angleRad));
            var y = (int)Math.Round(radius * Math.Sin(angleRad));

            return new Point(x, y);
        }

        public static Point PolarToCartesian(this Point point, int radius, double fi)
        {
            var x = (int)(point.X + radius * Math.Cos(fi));
            var y = (int)(point.Y + radius * Math.Sin(fi));
            return new Point(x, y);
        }
    }
}
