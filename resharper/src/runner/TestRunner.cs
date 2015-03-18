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

                Logger.LogVerbose("Setting current directory to {0}", assemblyFolder);

                Environment.CurrentDirectory = assemblyFolder;

                // Tell ReSharper about the shadow copy path, so it can clean up after abort. Xunit (1+2) will try and
                // clean up at the end of the run
                var shadowCopyPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
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
                    var testCases = GetTestCases(controller, taskProvider);

                    var run = new XunitTestRun(server, controller, taskProvider);
                    run.RunTests(testCases);
                }

                diagnosticMessages.Report(server);
            }
            catch (Exception e)
            {
                Logger.LogException(e);

                var message = e.Message + Environment.NewLine + Environment.NewLine + "(Note: xUnit.net ReSharper runner requires projects are built with xUnit.net 2.0.0-rc3-build2880)";
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

            Logger.LogVerbose("Starting discovery");
            Logger.LogVerbose("  Pre-enumerating theories: {0}", options.GetPreEnumerateTheoriesOrDefault());

            discoverer.Find(false, visitor, options);
            visitor.Finished.WaitOne();

            Logger.LogVerbose("Filtering test cases");
            return visitor.TestCases.Where(c => ShouldRunTestCase(taskProvider, c));
        }

        private static bool ShouldRunTestCase(TaskProvider taskProvider, ITestCase testCase)
        {
            var isRequestedMethod = IsRequestedMethod(testCase, taskProvider);
            var isDynamicMethod = IsDynamicMethod(testCase, taskProvider);

            var shouldRun = isRequestedMethod || isDynamicMethod;

            Logger.LogVerbose(" {0} test case {1}", shouldRun ? "Including" : "Excluding", testCase.Format());

            return shouldRun;
        }

        private static bool IsRequestedMethod(ITestCase testCase, TaskProvider taskProvider)
        {
            var typeName = testCase.TestMethod.TestClass.Class.Name;
            var methodName = testCase.TestMethod.Method.Name;
            var displayName = testCase.DisplayName ?? MakeDisplayName(typeName, methodName);

            if (TaskProvider.IsTheory(displayName, typeName, methodName))
            {
                var hasTheoryTask = taskProvider.HasTheoryTask(displayName, typeName, methodName);
                Logger.LogVerbose(" Theory test case is {0}a requested theory: {1}", hasTheoryTask ? string.Empty : "NOT ",
                    testCase.Format());
                return hasTheoryTask;
            }

            var hasMethodTask = taskProvider.HasMethodTask(typeName, methodName);
            Logger.LogVerbose(" Test case is {0}a requested method: {1}", hasMethodTask ? string.Empty : "NOT ",
                testCase.Format());
            return hasMethodTask;
        }

        private static bool IsDynamicMethod(ITestCase testCase, TaskProvider taskProvider)
        {
            var typeName = testCase.TestMethod.TestClass.Class.Name;
            var methodName = testCase.TestMethod.Method.Name;
            var displayName = testCase.DisplayName ?? MakeDisplayName(typeName, methodName);

            var classTaskInfo = taskProvider.GetClassTask(typeName);
            if (classTaskInfo == null)
            {
                Logger.LogVerbose(" Test case class unknown. Cannot be a dynamic test of a requested class. {0} - {1}", 
                    testCase.TestMethod.TestClass.Class.Name, testCase.Format());
                return false;
            }

            if (TaskProvider.IsTheory(displayName, typeName, methodName))
            {
                var isDynamicTheory = !classTaskInfo.ClassTask.IsKnownMethod(displayName.Replace(typeName + ".", string.Empty));
                Logger.LogVerbose(" Theory test case is {0}a requested theory: {1}", isDynamicTheory ? string.Empty : "NOT ",
                    testCase.Format());
                return isDynamicTheory;
            }

            var isDynamicMethod = !classTaskInfo.ClassTask.IsKnownMethod(methodName);
            Logger.LogVerbose(" Test case is {0}a dynamic method: {1}", isDynamicMethod ? string.Empty : "NOT ", testCase.Format());
            return !isDynamicMethod;
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
