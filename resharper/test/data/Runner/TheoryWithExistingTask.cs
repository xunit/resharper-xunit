using System;
using Xunit;
using Xunit.Extensions;

namespace Foo
{
    public class Theory
    {
        [Theory]
        [InlineData(42)]
        public void TestMethod(int value)
        {
        }
    }
}

// xunit2 doesn't define Xunit.Extensions
namespace Xunit.Extensions
{
}
