using Xunit;
using Xunit.Extensions;

namespace Foo
{
    public class UnicodeDataAttributeStrings
    {
        [Theory]
        [InlineData("Русский")]
        public void TestMethod(string value)
        {
        }
    }
}

// xunit2 doesn't define Xunit.Extensions
namespace Xunit.Extensions
{
}
