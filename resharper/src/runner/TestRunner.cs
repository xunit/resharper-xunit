using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    internal class TestRunner
    {
        private readonly RemoteTaskServer server;
        private readonly TaskExecutorConfiguration resharperConfiguration;

        public TestRunner(RemoteTaskServer server)
        {
            this.server = server;
            resharperConfiguration = server.Configuration;
        }

        public void Run(XunitTestAssemblyTask assemblyTask, TaskProvider taskProvider)
        {
            var priorCurrentDirectory = Environment.CurrentDirectory;
            DiagnosticMessages diagnosticMessages = null;
            try
            {
                // Use the assembly in the folder that the user has specified, or, if not, use the assembly location
                var assemblyFolder = GetAssemblyFolder(resharperConfiguration, assemblyTask);
                var assemblyPath = Path.Combine(assemblyFolder, GetFileName(assemblyTask.AssemblyLocation));
                var configFile = GetConfigFile(assemblyPath);
                var shadowCopyPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

                Environment.CurrentDirectory = assemblyFolder;

                // Tell ReSharper about the shadow copy path, so it can clean up after abort. Xunit (1+2) will try and
                // clean up at the end of the run
                server.SetTempFolderPath(shadowCopyPath);

                var xunitConfiguration = GetXunitConfiguration(assemblyPath, configFile);
                diagnosticMessages = new DiagnosticMessages(xunitConfiguration);

                // Pass in a null source information provider. We don't need or use it, so make sure xunit doesn't
                // create one for us, dragging in DIA and creating a new AppDomain. Let's keep things lightweight
                using (var controller = new XunitFrontController(assemblyPath, configFile,
                    resharperConfiguration.ShadowCopy, shadowCopyPath, new NullSourceInformationProvider(),
                    diagnosticMessages.Visitor))
                {
                    var testCases = GetTestCases(controller, taskProvider);

                    var run = new XunitTestRun(server, controller, taskProvider);
                    run.RunTests(testCases);
                }

                diagnosticMessages.Report(server);
            }
            catch (Exception e)
            {
                var message = e.Message + Environment.NewLine + Environment.NewLine + "(Note: xUnit.net ReSharper runner requires projects are built with xUnit.net 2.0.0-rc2-build2857)";
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

        private IEnumerable<ITestCase> GetTestCases(ITestFrameworkDiscoverer discoverer, TaskProvider taskProvider)
        {
            var visitor = new TestDiscoveryVisitor();
            var options = TestFrameworkOptions.ForDiscovery();
            discoverer.Find(false, visitor, options);
            visitor.Finished.WaitOne();
            return visitor.TestCases.Where(c => IsRequestedMethod(c, taskProvider) || IsDynamicMethod(c, taskProvider));
        }

        private static bool IsRequestedMethod(ITestCase testCase, TaskProvider taskProvider)
        {
            var typeName = testCase.TestMethod.TestClass.Class.Name;
            var methodName = testCase.TestMethod.Method.Name;
            var displayName = testCase.DisplayName ?? MakeDisplayName(typeName, methodName);
            if (TaskProvider.IsTheory(displayName, typeName, methodName))
                return taskProvider.HasTheoryTask(displayName, typeName, methodName);
            return taskProvider.HasMethodTask(typeName, methodName);
        }

        private static bool IsDynamicMethod(ITestCase testCase, TaskProvider taskProvider)
        {
            var typeName = testCase.TestMethod.TestClass.Class.Name;
            var methodName = testCase.TestMethod.Method.Name;
            var displayName = testCase.DisplayName ?? MakeDisplayName(typeName, methodName);

            var classTaskInfo = taskProvider.GetClassTask(typeName);
            if (classTaskInfo == null)
                return false;

            if (TaskProvider.IsTheory(displayName, typeName, methodName))
                return !classTaskInfo.ClassTask.IsKnownMethod(displayName.Replace(typeName + ".", string.Empty));
            return !classTaskInfo.ClassTask.IsKnownMethod(methodName);
        }

        private static string MakeDisplayName(string typeName, string methodName)
        {
            return typeName + "." + methodName;
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
