using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ImageEditor.Helpers
{
    public static class PointExtensions
    {
        public static List<Point> GetCircularPoints(this Point center,double radius, double angleInterval)
        {
            List<Point> points = new List<Point>();

            for (double interval = angleInterval; interval < 2 * Math.PI; interval += angleInterval)
            {
                int X = (int)(center.X + (radius * Math.Cos(interval)));
                int Y = (int)(center.Y + (radius * Math.Sin(interval)));

                points.Add(new Point(X, Y));
            }

            return points.Distinct().ToList();
        }
    }
}
