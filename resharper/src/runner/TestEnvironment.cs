using System;
using System.IO;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;
using XunitContrib.Runner.ReSharper.RemoteRunner.Logging;
using XunitContrib.Runner.ReSharper.RemoteRunner.Tasks;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class TestEnvironment
    {
        public TestEnvironment(XunitTestAssemblyTask assemblyTask)
        {
            // Use the assembly in the folder that the user has specified, or, if not, use the assembly location
            var assemblyFolder = GetAssemblyFolder(ResharperConfiguration, assemblyTask);
            AssemblyPath = Path.Combine(assemblyFolder, Path.GetFileName(assemblyTask.AssemblyLocation));
            ConfigPath = GetConfigFile(AssemblyPath);

            Logger.LogVerbose("Setting current directory to {0}", assemblyFolder);

            Environment.CurrentDirectory = assemblyFolder;

            // Tell ReSharper about the shadow copy path, so it can clean up after abort. Xunit (1+2) will try and
            // clean up at the end of the run
            ShadowCopyPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            TestAssemblyConfiguration = GetXunitConfiguration(AssemblyPath, ConfigPath);
            DiagnosticMessages = new DiagnosticMessages(TestAssemblyConfiguration);
        }

        public string AssemblyPath { get; private set; }
        public string ConfigPath { get; private set; }
        public bool ShadowCopy { get { return ResharperConfiguration.ShadowCopy; } }
        public string ShadowCopyPath { get; private set; }
        public DiagnosticMessages DiagnosticMessages { get; private set; }

        public TestAssemblyConfiguration TestAssemblyConfiguration { get; private set; }

        private static TaskExecutorConfiguration ResharperConfiguration
        {
            get { return TaskExecutor.Configuration; }
        }

        private static string GetAssemblyFolder(TaskExecutorConfiguration config, XunitTestAssemblyTask assemblyTask)
        {
            return string.IsNullOrEmpty(config.AssemblyFolder)
                ? Path.GetDirectoryName(assemblyTask.AssemblyLocation)
                : config.AssemblyFolder;
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

        private static TestAssemblyConfiguration GetXunitConfiguration(string assemblyPath, string configFile)
        {
            var configuration = ConfigReader.Load(assemblyPath, configFile);

            // We rely on the standard display name being ClassAndMethod in order to check for theories.
            // Don't let the user override it.
            // TODO: I suspect we can remove this when discovery happens in the editor
            configuration.MethodDisplay = TestMethodDisplay.ClassAndMethod;
            return configuration;
        }
    }
}