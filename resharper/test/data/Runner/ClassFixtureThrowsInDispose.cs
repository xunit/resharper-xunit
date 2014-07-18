using System;
using Xunit;

namespace Foo
{
    public class Fixture : IDisposable
    {
        public void Dispose()
        {
            throw new InvalidOperationException("Thrown in fixture Dispose");
        }
    }

    public class FixtureThrowsInDispose : IClassFixture<Fixture>
    {
        public FixtureThrowsInDispose(Fixture fixture)
        {
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

    public class Class2
    {
        [Fact]
        public void TestMethod()
        {
        }
    }
}
