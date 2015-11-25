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
        public TestEnvironment(XunitTestAssemblyTask assemblyTask, bool disableAllConcurrency)
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

            // The hosts require disabling concurrency (e.g. code coverage, continuous testing, dotMemoryUnit)
            DisableAllConcurrency = disableAllConcurrency;

            TestAssemblyConfiguration = GetXunitConfiguration(AssemblyPath, ConfigPath);
            MergeConfiguration();
            DiagnosticMessages = new DiagnosticMessages(TestAssemblyConfiguration.DiagnosticMessagesOrDefault);
        }

        public string AssemblyPath { get; private set; }
        public string ConfigPath { get; private set; }
        public string ShadowCopyPath { get; private set; }
        public bool DisableAllConcurrency { get; private set; }
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

        private void MergeConfiguration()
        {
            // Diagnostic messages can be enabled from xunit config, or if ReSharper is in internal debug mode
            if (ResharperConfiguration.IsInInternalDebug)
                TestAssemblyConfiguration.DiagnosticMessages = true;

            // TODO: Merge AppDomain flags?
            // xunit can specify AppDomain requirements. ReSharper has a flag (disabled by default) to run
            // each assembly in a separate AppDomain (but doesn't that require running assemblies in the same
            // process, which we don't do?)
            // TODO: Should we override the default AppDomainSupport?
            // Why do we want any AppDomains? Only useful for separating out loading different versions of
            // ReSharper's own assemblies? Would it cause problems with shadow copying?

            // TODO: Merge shadow copy flags?
            // I can tell if the xunit config was set, but not if the ReSharper one isn't. Which takes precedence?
            // (ReSharper's for now) Add diagnostic warning that the xunit
            TestAssemblyConfiguration.ShadowCopy = ResharperConfiguration.ShadowCopy;

            // Disable parallelisation if the test host requests it (e.g. for code coverage, continuous testing
            // or dotMemoryUnit). Note that we'll also need to disable async test message reporting
            if (DisableAllConcurrency)
            {
                TestAssemblyConfiguration.ParallelizeAssembly = false;
                TestAssemblyConfiguration.ParallelizeTestCollections = false;
            }
        }
    }
}