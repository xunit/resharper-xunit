using Xunit;
using Xunit.Extensions;

namespace Foo
{
    public class DisplayName
    {
        [Fact(DisplayName = "Test method")]
        public void TestMethod()
        {
        }
    }
}

// xunit2 doesn't define Xunit.Extensions
namespace Xunit.Extensions
{
}
