using Xunit;

namespace Foo
{
    public class SimpleFacts
    {
        [Fact]
        public void TestMethod()
        {
        }

        [Fact(Skip = "Skipped test")]
        public void SkippedMethod()
        {
        }
    }
}
