using System;
using Xunit;
using Xunit.Extensions;

namespace Foo
{
    // xunit1 doesn't support this and throws an exception
    public class AmbiguouslyNamedTestMethods
    {
        [Theory]
        [InlineData(12)]
        public void TestMethod1(int value)
        {
        }

        [Theory]
        [InlineData("foo")]
        public void TestMethod1(string value)
        {
        }
    }
}
