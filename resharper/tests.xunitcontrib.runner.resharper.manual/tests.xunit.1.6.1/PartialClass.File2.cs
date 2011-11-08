using Xunit;

namespace tests.xunit
{
    namespace PartialClasses
    {
        public partial class PartialClass
        {
            [Fact]
            public void TestInFile2()
            {
                Assert.Equal(3, 3);
            }
        }
    }
}