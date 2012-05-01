using System.Collections.Generic;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public abstract class SingleClassTestRunContext
    {
        protected readonly TestRun testRun;
        protected readonly Class testClass;

        protected SingleClassTestRunContext()
        {
            testRun = TestRun.SingleClassRun;
            testClass = testRun.DefaultClass();
        }

        protected void Run()
        {
            testRun.Run();
        }

        protected IEnumerable<TaskMessage> Messages { get { return testRun.Messages; }}
    }
}