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

                using (var executorWrapper = new ExecutorWrapper(assemblyPath, null, config.ShadowCopy))
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
