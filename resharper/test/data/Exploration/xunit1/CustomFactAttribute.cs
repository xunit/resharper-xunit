using Xunit;

namespace Foo
{
    public class MyFactAttribute : FactAttribute
    {
    }

    public class Tests
    {
        [MyFact]
        public void TestMethod()
        {
        }
    }
}
