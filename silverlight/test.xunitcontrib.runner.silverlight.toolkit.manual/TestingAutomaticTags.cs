using Xunit;

namespace test.xunitcontrib.runner.silverlight.toolkit.manual
{
    namespace QuickTests
    {
        public class DistinctName
        {
            [Fact]
            public void MyTestOne()
            {
                Assert.Equal(1, 1);
            }

            [Fact]
            public void MyTestTwo()
            {
                Assert.Equal(2, 2);
            }

            [Fact]
            public void MyTestThree()
            {
                Assert.Equal(3, 3);
            }
        }
    }

    namespace FancyTests
    {
        public class Interesting
        {
            [Fact]
            public void TestNumberOne()
            {
                Assert.Equal(1, 1);
            }

            [Fact]
            public void TestNumberTwo()
            {
                Assert.Equal(2, 2);
            }

            [Fact]
            public void TestNumberThree()
            {
                Assert.Equal(3, 3);
            }
        }
    }
}
