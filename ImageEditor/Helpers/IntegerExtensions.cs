using System.Drawing;

namespace ImageEditor.Helpers
{
    public static class IntegerExtensions
    {
        public static int TruncateRgb(this int value)
        {
            if (value < 0) value = 0;
            else if (value>255)value=255;
            return value;
        }

        public static int TruncateRgb(this double value)
        {
            if (value < 0) value = 0;
            else if (value > 255) value = 255;
            return (int)value;
        }

        public static Point[,] GetPointMatrixAroundTheCenter(this Point center,int offset)//i.e for matrix 3x3 offset=1, for 5x5 offset=2
        {
            var dim = offset * 2 + 1;
            var matrix = new Point[dim,dim];
            for (int i = center.X - offset,countX=0; i <= center.X + offset; i++,countX++)
            {                
                for (int j = center.Y - offset,countY=0; j <= center.Y + offset; j++,countY++)
                {
                    matrix[countY,countX]=new Point(i,j);
                }
            }

            return matrix;
        }
    }
}
