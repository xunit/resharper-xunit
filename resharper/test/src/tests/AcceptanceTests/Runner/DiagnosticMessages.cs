using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Runner
{
    // NOTE: Not included in 8.2, as 8.2 doesn't have a nice way of displaying
    // diagnostics and so throws up a dialog box (pun intended)
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