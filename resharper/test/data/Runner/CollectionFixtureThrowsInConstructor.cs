using System;
using Xunit;

namespace Foo
{
    public class Fixture
    {
        public Fixture()
        {
            throw new InvalidOperationException("Thrown in fixture Constructor");
        }
    }

    [CollectionDefinition("My collection")]
    public class Collection : ICollectionFixture<Fixture>
    {
    }

    [Collection("My collection")]
    public class FixtureThrowsInConstructor
    {
        public FixtureThrowsInConstructor(Fixture fixture)
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
