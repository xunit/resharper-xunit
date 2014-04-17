using Xunit;
using Xunit.Extensions;

namespace Foo
{
    public class FailingTheory
    {
        [Theory]
        [InlineData(12)]
        [InlineData(42)]
        public void TestMethod(int value)
        {
            Assert.Equal(12, value);
        }
    }
}

// xunit2 doesn't define Xunit.Extensions
namespace Xunit.Extensions
{
}
