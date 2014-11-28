using System;
using Xunit;

namespace Foo
{
    public class CapturesOutput
    {
        [Fact]
        public void OutputFromSuccessfulTest()
        {
            Console.Write("This is some output");
        }

        [Fact]
        public void OutputFromFailingTest()
        {
            Console.Write("This is also some output");
            Assert.Equal(12, 42);
        }
    }
}
