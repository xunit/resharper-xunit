using System;
using System.Reflection;
using Xunit;

namespace Foo
{
    public class CustomBeforeAfterThrows
    {
        [Fact]
        [CustomBeforeAfterTest]
        public void TestMethod1()
        {
        }

        [Fact]
        public void TestMethod2()
        {
        }

        public class CustomBeforeAfterTestAttribute : BeforeAfterTestAttribute
        {
            public override void Before(MethodInfo methodUnderTest)
            {
                throw new InvalidOperationException("Thrown in Before");
            }
        }
    }
}
