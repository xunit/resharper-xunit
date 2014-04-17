using System;
using System.Reflection;
using Xunit;
using Xunit.Sdk;

namespace Foo
{
    public class CustomBeforeAfterThrows
    {
        [Fact]
        [CustomBeforeAfterTest]
        public void TestMethod1()
        {
        }

        public void Foo(MethodInfo bar)
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
