using Xunit;

namespace tests.xunit.passing
{
    namespace PartialClasses
    {
        public partial class PartialClass
        {
            [Fact]
            public void TestInFile1()
            {
                Assert.Equal(2, 2);
            }
        }

        public partial class PartialClass
        {
            [Fact]
            public void TestInSecondInstanceInFile1()
            {
            }
        }
    }
}