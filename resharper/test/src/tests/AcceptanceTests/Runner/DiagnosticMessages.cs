using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Runner
{
    [Category("xunit2")]
    public class DiagnosticMessages : XunitTaskRunnerTestBase
    {
        public DiagnosticMessages()
            : base("xunit2")
        {
        }

        protected override string RelativeTestDataPath
        {
            get { return base.RelativeTestDataPath + @"Gold\"; }
        }

        [Test]
        public void TestDiagnosticMessagesReported()
        {
            DoOneTestWithStrictOrdering("DiagnosticMessagesReported");
        }

        [Test]
        public void TestDiagnosticMessagesIgnored()
        {
            DoOneTestWithStrictOrdering("DiagnosticMessagesIgnored");
        }
    }
}