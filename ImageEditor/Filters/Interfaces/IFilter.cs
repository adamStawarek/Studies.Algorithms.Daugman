using System.Drawing;

namespace ImageEditor.Filters.Interfaces
{
    public interface IFilter
    {
        Bitmap Filter(Bitmap image);
        string Name { get; }
    }
}
