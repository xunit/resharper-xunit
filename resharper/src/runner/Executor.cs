using System.Collections.Generic;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class Executor
    {
        private readonly ITestFrameworkExecutor executor;
        private readonly RunContext runContext;

        public Executor(ITestFrameworkExecutor executor, RunContext runContext)
        {
            this.executor = executor;
            this.runContext = runContext;
        }

        // Hmm. testCases is serialised, so can't be an arbitrary IEnumerable<>
        public void RunTests(IList<ITestCase> testCases)
        {
            // If there are no test cases, the Finished event is never set
            Debug.Assert(testCases.Count > 0);

            var visitor = new TestExecutionVisitor(runContext);
            // TODO: Set any execution options?
            var options = TestFrameworkOptions.ForExecution();
            executor.RunTests(testCases, visitor, options);
            visitor.Finished.WaitOne();
        }
    }
}