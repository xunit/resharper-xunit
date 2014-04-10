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
