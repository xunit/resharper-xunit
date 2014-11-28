using System;
using Xunit;
using Xunit.Extensions;

namespace Foo
{
    public class CapturesOutputFromTheory
    {
        [Theory]
        [InlineData(11)]
        [InlineData(42)]
        public void OutputFromSuccessfulTest(int value)
        {
            Console.Write("This is some output");
        }

        [Theory]
        [InlineData(12)]
        [InlineData(42)]
        public void OutputFromFailingTest(int value)
        {
            Console.Write("This is also some output");
            Assert.Equal(42, value);
        }
    }
}

// xunit2 doesn't define Xunit.Extensions
namespace Xunit.Extensions
{
}
