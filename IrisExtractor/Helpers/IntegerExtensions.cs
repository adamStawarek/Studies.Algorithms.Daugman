using System.Collections.Generic;
using System.Drawing;

namespace ImageEditor.Helpers
{
    public static class IntegerExtensions
    {
        public static List<Point> GetPointListAroundTheCenter(this Point center,int offset)//i.e for matrix 3x3 offset=1, for 5x5 offset=2
        {
            var lst = new List<Point>();
            for (int i = center.X - offset; i <= center.X + offset; i++)
            {                
                for (int j = center.Y - offset; j <= center.Y + offset; j++)
                {
                    lst.Add(new Point(i,j));
                }
            }

            return lst;
        }               
    }
}
