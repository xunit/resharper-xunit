using Xunit;
using Xunit.Extensions;

namespace Foo
{
    public class UnicodeDisplayName
    {
        [Fact(DisplayName = "Русский")]
        public void TestMethod()
        {
        }
    }
}

// xunit2 doesn't define Xunit.Extensions
namespace Xunit.Extensions
{
}
