using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Reusable.Exceptionize
{
    public static class DynamicExceptionExtensions
    {
        [ContractAnnotation("ex: null => halt; pattern: null => halt")]
        public static bool NameMatches([NotNull] this DynamicException ex, [NotNull, RegexPattern] string pattern)
        {
            if (ex == null) throw new ArgumentNullException(nameof(ex));
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));
                        
            return Regex.IsMatch(ex.GetType().Name, pattern, RegexOptions.IgnoreCase);
        }
        
//        [ContractAnnotation("ex: null => halt; name: null => halt")]
//        public static bool NameEquals([NotNull] this DynamicException ex, [NotNull] string name, StringComparison comparisonType = StringComparison.Ordinal)
//        {
//            if (ex == null) throw new ArgumentNullException(nameof(ex));
//            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
//            return ex.GetType().Name.Equals(name, comparisonType);
//        }
//
//        [ContractAnnotation("ex: null => halt")]
//        public static bool NameEquals([NotNull] this DynamicException ex, Enum errorCode, StringComparison comparisonType = StringComparison.Ordinal)
//        {
//            if (ex == null) throw new ArgumentNullException(nameof(ex));
//            return ex.NameEquals(errorCode.ToString(), comparisonType);
//        }

//        [ContractAnnotation("ex: null => halt")]
//        public static bool CreatedFrom<T>([NotNull] this DynamicException ex, StringComparison comparisonType = StringComparison.Ordinal) where T : IDynamicExceptionTemplate
//        {
//            if (ex == null) throw new ArgumentNullException(nameof(ex));
//            return ex.NameEquals(Regex.Replace(typeof(T).Name, "Template$", string.Empty), comparisonType);
//        }        
    }
}