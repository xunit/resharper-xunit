using System.Collections.Generic;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;
using XunitContrib.Runner.ReSharper.RemoteRunner.Logging;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class Executor
    {
        private readonly ITestFrameworkExecutor executor;
        private readonly RunContext runContext;
        private readonly TestEnvironment environment;

        public Executor(ITestFrameworkExecutor executor, RunContext runContext, TestEnvironment environment)
        {
            this.executor = executor;
            this.runContext = runContext;
            this.environment = environment;
        }

        // Hmm. testCases is serialised, so can't be an arbitrary IEnumerable<>
        public void RunTests(IList<ITestCase> testCases)
        {
            // If there are no test cases, the Finished event is never set
            Debug.Assert(testCases.Count > 0);

            var options = TestFrameworkOptions.ForExecution(environment.TestAssemblyConfiguration);

            Logger.LogVerbose("Starting execution");
            LogExecutionOptions(options);

            var visitor = new TestExecutionVisitor(runContext);
            executor.RunTests(testCases, visitor, options);
            visitor.Finished.WaitOne();
        }

        private static void LogExecutionOptions(ITestFrameworkExecutionOptions options)
        {
            if (!Logger.IsEnabled) return;

            Logger.LogVerbose("  Diagnostic messages: {0}", options.GetDiagnosticMessagesOrDefault());
            Logger.LogVerbose("  Disable parallelization: {0}", options.GetDisableParallelizationOrDefault());
            Logger.LogVerbose("  Max parallel threads: {0}", options.GetMaxParallelThreadsOrDefault());
            Logger.LogVerbose("  Synchronous message reporting: {0}", options.GetSynchronousMessageReportingOrDefault());
        }
    }
}