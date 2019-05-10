using System.Drawing;

namespace ImageEditor.Filters
{
    public struct FilterResult
    {
        public Bitmap Bitmap { get; set; }
        public int Radius { get; set; }
        public Point Pupil { get; set; }
    }
}