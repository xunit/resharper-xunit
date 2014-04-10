using System;
using Xunit;

namespace Foo
{
    public class ClassThrowsInConstructor
    {
        public ClassThrowsInConstructor()
        {
            throw new InvalidOperationException("Thrown from constructor");
        }

        [Fact]
        public void TestMethod1()
        {
        }

        [Fact]
        public void TestMethod2()
        {
        }
    }
}
