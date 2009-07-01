using Xunit;

namespace tests.multiple.assemblies
{
    // This is to ensure the runner can handle multiple assemblies, when running
    // and in discovery
    public class FromAnotherAssemblyTests
    {
        [Fact]
        public void SimpleXunitTest()
        {
            Assert.Equal(2, 2);
        }
    }
}
