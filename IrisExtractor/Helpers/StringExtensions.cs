namespace ImageEditor.Helpers
{
    public static class StringExtensions
    {
        public static string TrimRightFromChar(this string str)
        {
            var index = str.LastIndexOf('\\');
            return str.Substring(0, index);
        }
    }
}