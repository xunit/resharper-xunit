using System.Collections.Generic;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public static class EnumerableExtension
    {
        public static IEnumerable<T> Hide<T>(this IEnumerable<T> self)
        {
            // Shamelessly pinched from IX. Hides the object identity of self,
            // projecting into a singly enumerated enumerable
            foreach (var value in self)
                yield return value;
        }
    }
}