using Xunit;

namespace Foo
{
    public class SimpleFacts
    {
        [Fact]
        public void TestMethod()
        {
        }

        [Fact(SkipReason = "Skipped test")]
        public void SkippedMethod()
        {
        }
    }
}
