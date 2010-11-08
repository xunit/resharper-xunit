using System;
using System.Collections.Generic;

namespace Xunit
{
    internal static partial class Extensions
    {
        public static T Find<T>(this List<T> list, Predicate<T> predicate)
        {
            foreach (var item in list)
            {
                if (predicate(item))
                    return item;
            }
            return default(T);
        }
    }
}