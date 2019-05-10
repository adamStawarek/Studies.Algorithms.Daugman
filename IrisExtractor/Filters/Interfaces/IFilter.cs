using System.Drawing;

namespace ImageEditor.Filters.Interfaces
{
    public interface IFilter
    {
        FilterResult Filter(Bitmap image);
        string Name { get; }
    }
}
