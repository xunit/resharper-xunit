using Xunit;

namespace Foo
{
    public class SkippedFact
    {
        [Fact(Skip = "Skipped reason")]
        public void TestMethod()
        {
        }
    }
}
