using System;
using Xunit;

namespace tests.xunit.eyeball
{
    namespace TestsWithFixtures
    {
        public class MyFixtureClass : IDisposable
        {
            public int SharedState;

            public MyFixtureClass()
            {
                Console.WriteLine("MyFixtureClass() constructor");
            }

            public void Dispose()
            {
                Console.WriteLine("MyFixtureClass.Dispose()");
            }
        }

        // TEST: Each test should be wrapped in a call to the constructor and to Dispose
        // TEST: SetFixture is called for each test
        // TEST: The shared state in the fixture changes (note values might be out of order)
        public class TestsWithFixtures : IClassFixture<MyFixtureClass>, IDisposable
        {
            private readonly MyFixtureClass fixture;

            public TestsWithFixtures(MyFixtureClass data)
            {
                Console.WriteLine("TestWithFixtures() - constructor");
                fixture = data;
            }

            public void Dispose()
            {
                Console.WriteLine("TestsWithFixtures.Dispose()");
            }

            [Fact]
            public void Test01()
            {
                Console.WriteLine("Test01 - fixture data = {0}", fixture.SharedState++);
            }

            [Fact]
            public void Test02()
            {
                Console.WriteLine("Test02 - fixture data = {0}", fixture.SharedState++);
            }

            [Fact]
            public void Test03()
            {
                Console.WriteLine("Test03 - fixture data = {0}", fixture.SharedState++);
            }

            [Fact]
            public void Test04()
            {
                Console.WriteLine("Test04 - fixture data = {0}", fixture.SharedState++);
            }

            [Fact]
            public void Test05()
            {
                Console.WriteLine("Test01 - fixture data = {0}", fixture.SharedState++);
            }
        }

        public class TheoryTestsWithFixtures : IClassFixture<MyFixtureClass>, IDisposable
        {
            private readonly MyFixtureClass fixture;

            public TheoryTestsWithFixtures(MyFixtureClass data)
            {
                Console.WriteLine("TheoryTestsWithFixtures() - constructor");

                fixture = data;
            }

            public void Dispose()
            {
                Console.WriteLine("TheoryTestsWithFixtures.Dispose()");
            }

            [Theory]
            [InlineData(1)]
            [InlineData(2)]
            [InlineData(3)]
            [InlineData(4)]
            [InlineData(5)]
            public void AllTests(int index)
            {
                Console.WriteLine("Test{0} - fixture data = {1}", index, fixture.SharedState++);
                Assert.Equal(1, 1);
            }
        }
    }
}
