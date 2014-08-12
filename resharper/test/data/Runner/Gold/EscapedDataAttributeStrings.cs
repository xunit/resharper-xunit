using Xunit;
using Xunit.Extensions;

namespace Foo
{
    public class PassingTheory
    {
        [Theory]
        [InlineData("$ \\ \b \f \n \r \t \v")]
        public void TestMethod(string value)
        {
        }
    }
}

// xunit2 doesn't define Xunit.Extensions
namespace Xunit.Extensions
{
}
