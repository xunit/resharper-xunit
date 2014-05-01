using System;
using Xunit;

namespace tests.xunit.eyeball
{
    namespace TestClassLifecycle
    {
        // TEST: Each test should be wrapped in a call to the constructor and to Dispose
        // the test class only lasts for the lifetime of a single test method
        public class TestClassLifecycle : IDisposable
        {
            public TestClassLifecycle()
            {
                Console.WriteLine("TestClassLifecycle() - constructor");
            }

            public void Dispose()
            {
                Console.WriteLine("TestClassLifecycle.Dispose()");
            }

            [Fact(Skip = "xunit2 doesn't capture output")]
            public void Test01()
            {
                Console.WriteLine("Test01");
                Assert.Equal(1, 1);
            }

            [Fact(Skip = "xunit2 doesn't capture output")]
            public void Test02()
            {
                Console.WriteLine("Test02");
                Assert.Equal(1, 1);
            }

            [Fact(Skip = "xunit2 doesn't capture output")]
            public void Test03()
            {
                Console.WriteLine("Test03");
                Assert.Equal(1, 1);
            }

            [Fact(Skip = "xunit2 doesn't capture output")]
            public void Test04()
            {
                Console.WriteLine("Test04");
                Assert.Equal(1, 1);
            }

            [Fact(Skip = "xunit2 doesn't capture output")]
            public void Test05()
            {
                Console.WriteLine("Test05");
                Assert.Equal(1, 1);
            }
        }

        // TEST: Each test should be wrapped in a call to the constructor and to Dispose
        // the test class only lasts for the lifetime of a single test method row
        public class TheoryTestClassLifecycle : IDisposable
        {
            public TheoryTestClassLifecycle()
            {
                Console.WriteLine("TheoryTestClassLifecycle() - constructor");
            }

            public void Dispose()
            {
                Console.WriteLine("TheoryTestClassLifecycle.Dispose()");
            }

            [Theory(Skip = "xunit2 doesn't capture output")]
            [InlineData(1)]
            [InlineData(2)]
            [InlineData(3)]
            [InlineData(4)]
            [InlineData(5)]
            public void AllTests(int index)
            {
                Console.WriteLine("Test" + index);
                Assert.Equal(1, 1);
            }
        }
    }
}