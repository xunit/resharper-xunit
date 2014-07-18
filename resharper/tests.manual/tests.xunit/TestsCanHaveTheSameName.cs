using Xunit;

namespace tests.xunit.passing
{
    namespace TestsWithSameName
    {
        // xunit2 allows tests to have the same name. v1 treats this
        // as an ambiguous name exception and is a catastrophic error
        public class TestsCanHaveSameMethodName
        {
            [Fact]
            public void TheTest()
            {
                Assert.Equal(12, 23);
            }

            [Theory]
            [InlineData(42)]
            public void TheTest(int x)
            {
            }

            [Theory]
            [InlineData("Hello, world")]
            public void TheTest(string x)
            {
            }

            private void TheTest(string x, int y)
            {
            }
        }
    }
}