using Xunit;

namespace Foo
{
    public class Tests
    {
        [Fact(Skip = "Skipped test")]
        public void SkippedMethod()
        {
        }
    }
}
