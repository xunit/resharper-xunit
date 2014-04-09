using Xunit;
using Xunit.Extensions;

namespace Foo
{
    public class PassingTheory
    {
        [Theory]
        [InlineData(12)]
        [InlineData(42)]
        public void TestMethod(int value)
        {
        }
    }
}
