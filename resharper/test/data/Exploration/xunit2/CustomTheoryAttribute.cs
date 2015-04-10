using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Foo
{
    public class Tests
    {
        [MyTheory]
        public void TestMethod()
        {
        }
    }

    [XunitTestCaseDiscoverer("Foo.MyTheoryDiscoverer", "CustomTheoryAttribute")]
    public class MyTheoryAttribute : TheoryAttribute
    {
    }

    public class MyTheoryDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly IMessageSink diagnosticMessageSink;

        public MyTheoryDiscoverer(IMessageSink diagnosticMessageSink)
        {
            this.diagnosticMessageSink = diagnosticMessageSink;
        }

        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            return new XunitTestCase[]
            {
                new XunitTheoryTestCase(diagnosticMessageSink, TestMethodDisplay.ClassAndMethod, testMethod)
            };
        }
    }
}
