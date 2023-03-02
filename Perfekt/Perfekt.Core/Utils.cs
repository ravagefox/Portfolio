namespace Perfekt.Core
{
    public static class Utils
    {
        public static string GetSizeFormat(this double num, int divisor, params string[] suffixes)
        {
            if (num == 0.0) { return "0B"; }

            var suffix = string.Empty;

            var idx = 0;
            while (num >= divisor)
            {
                num /= divisor;
                suffix = suffixes[idx++];
            }

            return string.Format("{0,6:f}{1}", num, suffix);
        }
    }
}
