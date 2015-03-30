using System.Collections.Generic;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class XunitTestRun
    {
        private readonly RemoteTaskServer server;
        private readonly ITestFrameworkExecutor executor;
        private readonly RunContext runContext;

        public XunitTestRun(RemoteTaskServer server, ITestFrameworkExecutor executor, RunContext runContext)
        {
            this.server = server;
            this.executor = executor;
            this.runContext = runContext;
        }

        // Hmm. testCases is serialised, so can't be an arbitrary IEnumerable<>
        public void RunTests(IList<ITestCase> testCases)
        {
            // If there are no test cases, the Finished event is never set
            Debug.Assert(testCases.Count > 0);

            var logger = new ReSharperRunnerVisitor(runContext, server);
            // TODO: Set any execution options?
            var options = TestFrameworkOptions.ForExecution();
            executor.RunTests(testCases, logger, options);
            logger.Finished.WaitOne();
        }
    }
}