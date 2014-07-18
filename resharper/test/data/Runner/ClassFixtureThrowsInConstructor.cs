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

    public class FixtureThrowsInConstructor : IClassFixture<Fixture>
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

    public class Class2
    {
        [Fact]
        public void TestMethod()
        {
        }
    }
}
