using System.Globalization;

namespace Xunit
{
    internal static partial class Extensions
    {
        public static string ToUpperInvariant(this string value)
        {
            return value.ToUpper(CultureInfo.InvariantCulture);
        }
    }
}