using System;
using Xunit;

namespace Foo
{
    public class MultipleFacts
    {
        [Fact]
        public void TestMethod1()
        {
            Console.Write("Output from TestMethod1");
        }

        [Fact]
        public void TestMethod2()
        {
            Console.Write("Output from TestMethod2");
        }

        [Fact]
        public void FailingTest()
        {
            Assert.Equal(12, 42);
        }
    }
}
