using System.Linq;

namespace ImageEditor.Helpers
{
    public static class StringExtensions
    {
        public static string TrimRightFromChar(this string str)
        {
            var index = str.LastIndexOf('\\');
            return str.Substring(0, index);
        }

        public static bool IsInTrainSet(this string str)
        {
            var isInTrainSet = !str.Split('\\').Last().EndsWith("1.jpg");
            return isInTrainSet;
        }       
    }
}