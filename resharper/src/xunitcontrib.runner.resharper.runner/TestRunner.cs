using System;
using System.IO;
using System.Linq;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    internal class TestRunner : RecursiveRemoteTaskRunnerBase
    {
        public TestRunner(IRemoteTaskServer server) : base(server)
        {
        }

        public override void ExecuteRecursive(TaskExecutionNode node)
        {
            var assemblyTask = (XunitTestAssemblyTask)node.RemoteTask;
            var config = Server.GetConfiguration();

            var priorCurrentDirectory = Environment.CurrentDirectory;
            try
            {
                // Use the assembly in the folder that the user has specified, or, if not, use the assembly location
                var assemblyFolder = GetAssemblyFolder(config, assemblyTask);
                var assemblyPath = Path.Combine(assemblyFolder, GetFileName(assemblyTask.AssemblyLocation));

                Environment.CurrentDirectory = assemblyFolder;

                // If we just pass null for the config file, the AppDomain will load {assembly}.config if it exists,
                // or use the config file of this app domain, which will usually be JetBrains.ReSharper.TaskRunner.*.exe.config.
                // If we specify the name directly, it will just use it, or have no configuration, with no fallback.
                // This is good because it stops the TaskRunner.exe config leaking into your tests. For example, that
                // file redirects all ReSharper assemblies to the current version. When the default AppDomain loads our
                // code, the assemblies are successfully redirected, and we use the latest version. If the new AppDomain
                // uses the same redirects, and the test assembly references resharper assemblies (e.g. xunitcontrib tests!)
                // the redirects are applied, but the new AppDomain can't find the newer assemblies, and throws
                var configFile = assemblyPath + ".config";

                using (var executorWrapper = new ExecutorWrapper(assemblyPath, configFile, config.ShadowCopy))
                {
                    var run = new XunitTestRun(Server, executorWrapper);

                    foreach (var classNode in node.Children)
                    {
                        run.AddClass((XunitTestClassTask) classNode.RemoteTask,
                                     classNode.Children.Select(n => (XunitTestMethodTask) n.RemoteTask));
                    }

                    run.RunTests();
                }
            }
            finally
            {
                Environment.CurrentDirectory = priorCurrentDirectory;
            }
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
