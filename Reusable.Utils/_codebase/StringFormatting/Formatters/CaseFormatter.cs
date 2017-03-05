using System;
using System.Text.RegularExpressions;

namespace Reusable.StringFormatting.Formatters
{
    public class CaseFormatter : CustomFormatter
    {
        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format)) { return null; }

            if (Regex.IsMatch(format, "ToUpper|Upper|U", RegexOptions.IgnoreCase))
            {
                return string.Format(formatProvider, "{0}", arg).ToUpper();
            }

            if (Regex.IsMatch(format, "ToLower|Lower|L", RegexOptions.IgnoreCase))
            {
                return string.Format(formatProvider, "{0}", arg).ToLower();
            }

            return null;
        }
    }
}