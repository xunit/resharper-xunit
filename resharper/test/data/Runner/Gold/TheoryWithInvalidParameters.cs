using Xunit;
using Xunit.Extensions;

namespace Foo
{
    public class TheoryWithInvalidParameters
    {
        [Theory]
        [InlineData(12)]
        [InlineData(42)]
        public void TestMethod(string value1, double value2)
        {
        }
    }
}

// xunit2 doesn't define Xunit.Extensions
namespace Xunit.Extensions
{
}
