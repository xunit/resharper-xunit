using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Foo
{
    public class CustomTestOrderer : ITestCaseOrderer
    {
        private readonly IMessageSink messageSink;

        public CustomTestOrderer(IMessageSink messageSink)
        {
            this.messageSink = messageSink;
        }

        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
            where TTestCase : ITestCase
        {
            messageSink.OnMessage(new DiagnosticMessage("Reporting diagnostic message"));
            return testCases;
        }
    }

    [TestCaseOrderer("Foo.CustomTestOrderer", "DiagnosticMessagesIgnored.xunit2")]
    public class TestCase
    {
        [Fact]
        public void TestMethod()
        {
        }
    }
}
