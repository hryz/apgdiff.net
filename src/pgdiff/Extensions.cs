using System;

namespace pgdiff
{
    public static class Extensions
    {
        public static bool EqualsIgnoreCase(this string src, string value)
        {
            return src.Equals(value, StringComparison.OrdinalIgnoreCase);
        }
    }
}
