using System;
using Xunit;

namespace Foo
{
    public class Fixture
    {
        public Fixture()
        {
            throw new InvalidOperationException("Thrown in fixture constructor");
        }
    }

    public class FixtureThrowsInConstructor : IUseFixture<Fixture>
    {
        public void SetFixture(Fixture fixture)
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
