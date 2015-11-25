using System;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.Util;
using Xunit;
using XunitContrib.Runner.ReSharper.RemoteRunner.Logging;
using XunitContrib.Runner.ReSharper.RemoteRunner.Tasks;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class TestRunner
    {
        private readonly IRemoteTaskServer server;

        public TestRunner(IRemoteTaskServer server)
        {
            this.server = server;
            RunContext = new RunContext(server);
        }

        public RunContext RunContext { get; private set; }

        public void Run(XunitTestAssemblyTask assemblyTask, bool disableAllConcurrency)
        {
            var environment = new TestEnvironment(assemblyTask, disableAllConcurrency);
            try
            {
                if (environment.TestAssemblyConfiguration.ShadowCopyOrDefault)
                    server.SetTempFolderPath(environment.ShadowCopyPath);

                using (var controller = GetFrontController(environment))
                {
                    var discoverer = new Discoverer(controller, RunContext, environment);
                    var testCases = discoverer.GetTestCases();

                    if (testCases.Count == 0)
                        throw new InternalErrorException("Unexpected: Unable to find any matching test cases");

                    if (testCases.Count > 0)
                    {
                        RunContext.AddRange(testCases);

                        var executor = new Executor(controller, RunContext, environment);
                        executor.RunTests(testCases);
                    }
                }

                environment.DiagnosticMessages.Report(server);
            }
            catch (Exception e)
            {
                ReportException(assemblyTask, environment, e);
            }
        }

        public void Abort()
        {
            RunContext.Abort();
        }

        private static IFrontController GetFrontController(TestEnvironment environment)
        {
            var configuration = environment.TestAssemblyConfiguration;
            return new XunitFrontController(configuration.AppDomainOrDefault, environment.AssemblyPath, environment.ConfigPath,
                configuration.ShadowCopyOrDefault, environment.ShadowCopyPath, new NullSourceInformationProvider(),
                environment.DiagnosticMessages.Visitor);
        }

        private void ReportException(XunitTestAssemblyTask assemblyTask, TestEnvironment environment, Exception e)
        {
            Logger.LogException(e);

            var description = "Exception: " + e;
            if (environment.DiagnosticMessages != null && environment.DiagnosticMessages.HasMessages)
            {
                description = string.Format("{0}{1}{1}Diagnostic Messages:{1}{2}", description,
                    Environment.NewLine, environment.DiagnosticMessages.Messages);
            }
            server.ShowNotification("Unable to run xUnit.net tests - " + e.Message, description);

            // This doesn't help - assemblyTask doesn't map to anything in the tree...
            server.TaskException(assemblyTask, new[] {new TaskException(e)});
        }
    }
}
