using Xunit;

namespace Foo
{
    public class FailingFact
    {
        [Fact]
        public void TestMethod()
        {
            Assert.Equal(12, 42);
        }
    }
}
