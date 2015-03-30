using System;
using System.IO;
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
        private readonly TaskExecutorConfiguration resharperConfiguration;

        public TestRunner(IRemoteTaskServer server, RunContext runContext)
        {
            this.server = server;
            this.runContext = runContext;
            resharperConfiguration = TaskExecutor.Configuration;
        }

        public void Run(XunitTestAssemblyTask assemblyTask, TaskExecutionNode assemblyTaskNode)
        {
            foreach (var classNode in assemblyTaskNode.Children)
            {
                runContext.Add((XunitTestClassTask)classNode.RemoteTask);
                foreach (var methodNode in classNode.Children)
                {
                    runContext.Add((XunitTestMethodTask)methodNode.RemoteTask);
                    foreach (var theoryNode in methodNode.Children)
                    {
                        runContext.Add((XunitTestTheoryTask)theoryNode.RemoteTask);
                    }
                }
            }

            var priorCurrentDirectory = Environment.CurrentDirectory;
            DiagnosticMessages diagnosticMessages = null;
            try
            {
                // Use the assembly in the folder that the user has specified, or, if not, use the assembly location
                var assemblyFolder = GetAssemblyFolder(resharperConfiguration, assemblyTask);
                var assemblyPath = Path.Combine(assemblyFolder, GetFileName(assemblyTask.AssemblyLocation));
                var configFile = GetConfigFile(assemblyPath);

                Logger.LogVerbose("Setting current directory to {0}", assemblyFolder);

                Environment.CurrentDirectory = assemblyFolder;

                // Tell ReSharper about the shadow copy path, so it can clean up after abort. Xunit (1+2) will try and
                // clean up at the end of the run
                var shadowCopyPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

                // TODO: Only when shadow copy enabled?
                // Fix this up after rewrite so we don't cause trouble with tests
                if (resharperConfiguration.ShadowCopy)
                {
                    Logger.LogVerbose("Shadow copy enabled: {0}", shadowCopyPath);
                    server.SetTempFolderPath(shadowCopyPath);
                }

                var xunitConfiguration = GetXunitConfiguration(assemblyPath, configFile);
                diagnosticMessages = new DiagnosticMessages(xunitConfiguration);

                // Pass in a null source information provider. We don't need or use it, so make sure xunit doesn't
                // create one for us, dragging in DIA and creating a new AppDomain. Let's keep things lightweight
                using (var controller = new XunitFrontController(assemblyPath, configFile,
                    resharperConfiguration.ShadowCopy, shadowCopyPath, new NullSourceInformationProvider(),
                    diagnosticMessages.Visitor))
                {
                    var discovery = new Discoverer(runContext);
                    var testCases = discovery.GetTestCases(controller);
                    if (testCases.Count == 0)
                    {
                        // TODO: Report something? Make it a diagnostic message
                        return;
                    }

                    foreach (var testCase in testCases)
                        runContext.Add(testCase);

                    var run = new XunitTestRun(controller, runContext);
                    run.RunTests(testCases);
                }

                diagnosticMessages.Report(server);
            }
            catch (Exception e)
            {
                Logger.LogException(e);

                var message = e.Message + Environment.NewLine + Environment.NewLine + "(Note: xUnit.net ReSharper runner requires projects are built with xUnit.net 2.0.0 RTM or later)";
                var description = "Exception: " + e.ToString();
                if (diagnosticMessages != null && diagnosticMessages.HasMessages)
                {
                    description = string.Format("{0}{1}{1}Diagnostic Messages:{1}{2}", description, Environment.NewLine,
                        diagnosticMessages.Messages);
                }
                server.ShowNotification("Unable to run xUnit.net tests - " + message, description);

                // This doesn't help - assemblyTask doesn't map to anything in the tree...
                server.TaskException(assemblyTask, new[] { new TaskException(e) });
            }
            finally
            {
                Environment.CurrentDirectory = priorCurrentDirectory;
            }
        }

        private static TestAssemblyConfiguration GetXunitConfiguration(string assemblyPath, string configFile)
        {
            return ConfigReader.Load(assemblyPath, configFile);
        }

        private static string GetConfigFile(string assemblyPath)
        {
            // If we just pass null for the config file, the AppDomain will load {assembly}.config if it exists,
            // or use the config file of this app domain, which will usually be JetBrains.ReSharper.TaskRunner.*.exe.config.
            // If we specify the name directly, it will just use it, or have no configuration, with no fallback.
            // This is good because it stops the TaskRunner.exe config leaking into your tests. For example, that
            // file redirects all ReSharper assemblies to the current version. When the default AppDomain loads our
            // code, the assemblies are successfully redirected, and we use the latest version. If the new AppDomain
            // uses the same redirects, and the test assembly references resharper assemblies (e.g. xunitcontrib tests!)
            // the redirects are applied, but the new AppDomain can't find the newer assemblies, and throws
            return assemblyPath + ".config";
        }

        private static string GetAssemblyFolder(TaskExecutorConfiguration config, XunitTestAssemblyTask assemblyTask)
        {
            return string.IsNullOrEmpty(config.AssemblyFolder)
                       ? GetDirectoryName(assemblyTask.AssemblyLocation)
                       : config.AssemblyFolder;
        }

        private static string GetDirectoryName(string filepath)
        {
            return Path.GetDirectoryName(filepath);
        }

        private static string GetFileName(string filepath)
        {
            return Path.GetFileName(filepath);
        }
    }
}
