using System;
using Xunit;
using Xunit.Extensions;

namespace Foo
{
    // xunit1 doesn't support this. xunit2 does!
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

// xunit2 moves Theory to Xunit, meaning Xunit.Extensions doesn't exist
namespace Xunit.Extensions
{
}
