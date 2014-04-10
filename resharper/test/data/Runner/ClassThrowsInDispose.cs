using System;
using Xunit;

namespace Foo
{
    public class ClassThrowsInDispose : IDisposable
    {
        public void Dispose()
        {
            throw new InvalidOperationException("Thrown from Dispose");
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
