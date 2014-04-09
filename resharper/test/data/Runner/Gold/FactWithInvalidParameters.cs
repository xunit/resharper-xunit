using Xunit;

namespace Foo
{
    public class FactWithInvalidParameters
    {
        [Fact]
        public void TestMethod(int invalidParameter)
        {
        }
    }
}
