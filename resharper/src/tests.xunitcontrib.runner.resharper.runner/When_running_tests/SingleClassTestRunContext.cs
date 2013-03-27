using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public abstract class SingleClassTestRunContext
    {
        protected readonly TestRun testRun;
        protected readonly Class testClass;

        protected SingleClassTestRunContext()
        {
            testRun = TestRun.SingleClassRun;
            testClass = testRun.Classes.Single();
        }

        protected void Run()
        {
            testRun.Run(ResultInspector);
        }

        protected IEnumerable<TaskMessage> Messages { get { return testRun.Messages; }}
        protected Func<ITestResult, ITestResult> ResultInspector { get; set; }
    }
}