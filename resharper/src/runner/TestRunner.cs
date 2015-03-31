using System;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;
using XunitContrib.Runner.ReSharper.RemoteRunner.Logging;
using XunitContrib.Runner.ReSharper.RemoteRunner.Tasks;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    internal class TestRunner
    {
        private readonly IRemoteTaskServer server;
        private readonly RunContext runContext;

        public TestRunner(IRemoteTaskServer server, RunContext runContext)
        {
            this.server = server;
            this.runContext = runContext;
        }

        public void Run(XunitTestAssemblyTask assemblyTask)
        {
            var environment = new TestEnvironment(assemblyTask);
            try
            {
                if (environment.ShadowCopy)
                    server.SetTempFolderPath(environment.ShadowCopyPath);

                using (var controller = GetFrontController(environment))
                {
                    var discoverer = new Discoverer(controller, runContext);
                    var testCases = discoverer.GetTestCases();

                    // TODO: Report something if no test cases?
                    if (testCases.Count > 0)
                    {
                        runContext.AddRange(testCases);

                        var executor = new XunitTestRun(controller, runContext);
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

        private static IFrontController GetFrontController(TestEnvironment environment)
        {
            return new XunitFrontController(environment.AssemblyPath, environment.ConfigPath,
                environment.ShadowCopy, environment.ShadowCopyPath, new NullSourceInformationProvider(),
                environment.DiagnosticMessages.Visitor);
        }

        private void ReportException(XunitTestAssemblyTask assemblyTask, TestEnvironment environment, Exception e)
        {
            Logger.LogException(e);

            var message = e.Message + Environment.NewLine + Environment.NewLine +
                          "(Note: xUnit.net ReSharper runner requires projects are built with xUnit.net 2.0.0 RTM or later)";
            var description = "Exception: " + e;
            if (environment.DiagnosticMessages != null && environment.DiagnosticMessages.HasMessages)
            {
                description = string.Format("{0}{1}{1}Diagnostic Messages:{1}{2}", description,
                    Environment.NewLine, environment.DiagnosticMessages.Messages);
            }
            server.ShowNotification("Unable to run xUnit.net tests - " + message, description);

            // This doesn't help - assemblyTask doesn't map to anything in the tree...
            server.TaskException(assemblyTask, new[] {new TaskException(e)});
        }
    }
}
