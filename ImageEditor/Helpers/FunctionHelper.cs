using OxyPlot;

namespace ImageEditor.Helpers
{
    public class FunctionHelper
    {
        public static int GetThirdPointYValue(DataPoint p1, DataPoint p2,int p3X)
        {
            if (p3X == (int)p1.X) return (int)p1.Y;
            if (p3X == (int)p2.X) return (int)p2.Y;
            var y=((p3X - p1.X) / (p2.X - p1.X)) * (p2.Y - p1.Y) + p1.Y;
            return (int)y;
        }
    }
}
