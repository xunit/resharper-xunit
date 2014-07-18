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

    [CollectionDefinition("My collection")]
    public class Collection : ICollectionFixture<Fixture>
    {
    }

    [Collection("My collection")]
    public class FixtureThrowsInDispose
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

    [Collection("Other collection")]
    public class Class2
    {
        [Fact]
        public void TestMethod()
        {
        }
    }
}
